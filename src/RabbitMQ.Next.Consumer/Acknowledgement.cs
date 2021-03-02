using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Acknowledgement : IAcknowledgement
    {
        private readonly IChannel channel;
        private bool isDisposed;
        
        public Acknowledgement(IChannel channel)
        {
            this.channel = channel;
        }

        public ValueTask DisposeAsync()
        {
            this.isDisposed = true;
            return default;
        }

        public ValueTask AckAsync(ulong deliveryTag, bool multiple = false)
        {
            this.CheckDisposed();

            return new ValueTask(this.channel.SendAsync(new AckMethod(deliveryTag, multiple)));
        }


        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            this.CheckDisposed();

            return new ValueTask(this.channel.SendAsync(new NackMethod(deliveryTag, false, requeue)));
        }

        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Acknowledgement));
            }
        }
    }
}