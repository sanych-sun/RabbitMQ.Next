using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class DefaultAcknowledgement : IAcknowledgement
    {
        private readonly IChannel channel;
        
        public DefaultAcknowledgement(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            this.channel = channel;
        }

        public ValueTask DisposeAsync() => default;

        public ValueTask AckAsync(ulong deliveryTag)
            => this.channel.SendAsync(new AckMethod(deliveryTag, false));

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
            => this.channel.SendAsync(new NackMethod(deliveryTag, false, requeue));
    }
}