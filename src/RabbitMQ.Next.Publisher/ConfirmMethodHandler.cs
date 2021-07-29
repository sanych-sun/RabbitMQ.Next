using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal class ConfirmMethodHandler : IMethodHandler
    {
        private static readonly TaskCompletionSource<bool> PositiveCompletedTcs;
        private static readonly TaskCompletionSource<bool> NegativeCompletedTcs;
        private ulong lastMultipleAck;
        private ulong lastMultipleNack;

        static ConfirmMethodHandler()
        {
            PositiveCompletedTcs = new TaskCompletionSource<bool>();
            PositiveCompletedTcs.SetResult(true);

            NegativeCompletedTcs = new TaskCompletionSource<bool>();
            NegativeCompletedTcs.SetResult(false);
        }

        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> pendingConfirms;

        public ConfirmMethodHandler()
        {
            this.pendingConfirms = new ConcurrentDictionary<ulong, TaskCompletionSource<bool>>();
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

        ValueTask<bool> IMethodHandler.HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method is AckMethod ack)
            {
                if (ack.Multiple)
                {
                    Interlocked.Exchange(ref this.lastMultipleAck, ack.DeliveryTag);
                    this.AckMultiple(ack.DeliveryTag, true);
                }
                else
                {
                    this.AckSingle(ack.DeliveryTag, true);
                }

                return new ValueTask<bool>(true);
            }

            if (method is NackMethod nack)
            {
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