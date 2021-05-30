using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal class ConfirmMethodHandler : IMethodHandler
    {
        private readonly AsyncManualResetEvent confirmations;
        private readonly ConcurrentDictionary<ulong, bool> responses;
        private ulong lastMultipleAckTag;
        private ulong lastMultipleNackTag;

        public ConfirmMethodHandler()
        {
            this.confirmations = new AsyncManualResetEvent();
            this.responses = new ConcurrentDictionary<ulong, bool>();
        }

        public async ValueTask<bool> WaitForConfirmAsync(ulong deliveryTag)
        {
            while(true)
            {
                if (this.responses.TryRemove(deliveryTag, out var confirmed))
                {
                    return confirmed;
                }

                if (deliveryTag <= this.lastMultipleAckTag)
                {
                    return true;
                }

                if (deliveryTag <= this.lastMultipleNackTag)
                {
                    return false;
                }

                await this.confirmations.WaitAsync();
            }
        }

        ValueTask<bool> IMethodHandler.HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method is AckMethod ack)
            {
                if (ack.Multiple || this.lastMultipleAckTag + 1 == ack.DeliveryTag)
                {
                    this.lastMultipleAckTag = ack.DeliveryTag;
                }
                else
                {
                    this.responses.TryAdd(ack.DeliveryTag, true);
                }
                this.confirmations.Set();
                this.confirmations.Reset();

                return new ValueTask<bool>(true);
            }

            if (method is NackMethod nack)
            {
                if (nack.Multiple)
                {
                    this.lastMultipleNackTag = nack.DeliveryTag;
                }
                else
                {
                    this.responses.TryAdd(nack.DeliveryTag, false);
                }
                this.confirmations.Set();
                this.confirmations.Reset();

                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        }
    }
}