using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
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

        bool IFrameHandler.HandleMethodFrame(MethodId methodId, ReadOnlySpan<byte> payload)
        {
            switch (methodId)
            {
                case MethodId.BasicAck:
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
                case MethodId.BasicNack:
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
                default:
                    return false;
            }
        }

        ValueTask<bool> IFrameHandler.HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);

        void IFrameHandler.Release(Exception ex)
        {
            foreach (var task in this.pendingConfirms)
            {
                if (ex == null)
                {
                    task.Value.TrySetCanceled();
                }
                else
                {
                    task.Value.TrySetException(ex);
                }
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

            foreach (var (key, value) in items)
            {
                value.TrySetResult(isPositive);
                this.pendingConfirms.TryRemove(key, out _);
            }
        }
    }
}