using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal class ConfirmFrameHandler : IFrameHandler
    {
        private static readonly TaskCompletionSource<bool> PositiveCompletedTcs;
        private static readonly TaskCompletionSource<bool> NegativeCompletedTcs;

        private readonly IMethodParser<AckMethod> ackMethodParser;
        private readonly IMethodParser<NackMethod> nackMethodParser;
        private ulong lastMultipleAck;
        private ulong lastMultipleNack;

        static ConfirmFrameHandler()
        {
            PositiveCompletedTcs = new TaskCompletionSource<bool>();
            PositiveCompletedTcs.SetResult(true);

            NegativeCompletedTcs = new TaskCompletionSource<bool>();
            NegativeCompletedTcs.SetResult(false);
        }

        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> pendingConfirms;

        public ConfirmFrameHandler(IMethodRegistry registry)
        {
            this.pendingConfirms = new ConcurrentDictionary<ulong, TaskCompletionSource<bool>>();
            this.ackMethodParser = registry.GetParser<AckMethod>();
            this.nackMethodParser = registry.GetParser<NackMethod>();
        }

        public ValueTask<bool> WaitForConfirmAsync(ulong deliveryTag)
        {
            var lastMultNack = Interlocked.Read(ref this.lastMultipleNack);
            if (deliveryTag <= lastMultNack)
            {
                return new ValueTask<bool>(false);
            }

            var lastMultAck = Interlocked.Read(ref this.lastMultipleAck);
            if (deliveryTag <= lastMultAck)
            {
                return new ValueTask<bool>(true);
            }

            var tcs = this.pendingConfirms.GetOrAdd(deliveryTag, _ => new TaskCompletionSource<bool>());
            return new ValueTask<bool>(tcs.Task);
        }

        ValueTask<bool> IFrameHandler.HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId == MethodId.BasicAck)
            {
                var ack = this.ackMethodParser.Parse(payload);
                if (ack.Multiple)
                {
                    Interlocked.Exchange(ref this.lastMultipleAck, ack.DeliveryTag);
                    this.AckMultiple(ack.DeliveryTag, true);
                }
                else
                {
                    this.AckSingle(ack.DeliveryTag, true);
                }

                return new (true);
            }

            if (methodId == MethodId.BasicNack)
            {
                var nack = this.nackMethodParser.Parse(payload);
                if (nack.Multiple)
                {
                    Interlocked.Exchange(ref this.lastMultipleNack, nack.DeliveryTag);
                    this.AckMultiple(nack.DeliveryTag, false);
                }
                else
                {
                    this.AckSingle(nack.DeliveryTag, false);
                }

                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        }

        ValueTask<bool> IFrameHandler.HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);

        private void AckSingle(ulong deliveryTag, bool isPositive)
        {
            if (!this.pendingConfirms.TryRemove(deliveryTag, out var tcs))
            {
                tcs = this.pendingConfirms.GetOrAdd(deliveryTag, isPositive ? PositiveCompletedTcs : NegativeCompletedTcs);
            }

            tcs.TrySetResult(isPositive);
        }

        private void AckMultiple(ulong deliveryTag, bool isPositive)
        {
            var items = this.pendingConfirms.Where(t => t.Key <= deliveryTag).ToArray();

            foreach (var item in items)
            {
                item.Value.TrySetResult(isPositive);
                this.pendingConfirms.TryRemove(item.Key, out var _);
            }
        }
    }
}