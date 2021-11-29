using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> pendingConfirms;
        private readonly IMethodParser<AckMethod> ackMethodParser;
        private readonly IMethodParser<NackMethod> nackMethodParser;

        static ConfirmFrameHandler()
        {
            PositiveCompletedTcs = new TaskCompletionSource<bool>();
            PositiveCompletedTcs.SetResult(true);

            NegativeCompletedTcs = new TaskCompletionSource<bool>();
            NegativeCompletedTcs.SetResult(false);
        }

        public ConfirmFrameHandler(IMethodRegistry registry)
        {
            this.pendingConfirms = new ConcurrentDictionary<ulong, TaskCompletionSource<bool>>();
            this.ackMethodParser = registry.GetParser<AckMethod>();
            this.nackMethodParser = registry.GetParser<NackMethod>();
        }

        public ValueTask<bool> WaitForConfirmAsync(ulong deliveryTag)
        {
            var tcs = this.pendingConfirms.GetOrAdd(deliveryTag, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));
            if (tcs.Task.IsCompleted)
            {
                this.pendingConfirms.TryRemove(deliveryTag, out _);
            }

            return new ValueTask<bool>(tcs.Task);
        }

        bool IFrameHandler.HandleMethodFrame(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId == MethodId.BasicAck)
            {
                var ack = this.ackMethodParser.Parse(payload);
                if (ack.Multiple)
                {
                    this.AckMultiple(ack.DeliveryTag, true);
                }
                else
                {
                    this.AckSingle(ack.DeliveryTag, true);
                }

                return true;
            }

            if (methodId == MethodId.BasicNack)
            {
                var nack = this.nackMethodParser.Parse(payload);
                if (nack.Multiple)
                {
                    this.AckMultiple(nack.DeliveryTag, false);
                }
                else
                {
                    this.AckSingle(nack.DeliveryTag, false);
                }

                return true;
            }

            return false;
        }

        ValueTask<bool> IFrameHandler.HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);

        void IFrameHandler.Reset()
        {
            foreach (var task in this.pendingConfirms)
            {
                task.Value.SetCanceled();
            }
            this.pendingConfirms.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AckSingle(ulong deliveryTag, bool isPositive)
        {
            if (!this.pendingConfirms.TryRemove(deliveryTag, out var tcs))
            {
                tcs = this.pendingConfirms.GetOrAdd(deliveryTag, isPositive ? PositiveCompletedTcs : NegativeCompletedTcs);
            }

            tcs.TrySetResult(isPositive);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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