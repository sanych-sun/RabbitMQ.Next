using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

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

    public async ValueTask AckAsync(ulong deliveryTag)
        => await this.channel.SendAsync(new AckMethod(deliveryTag, false));

    public async ValueTask NackAsync(ulong deliveryTag, bool requeue)
        => await this.channel.SendAsync(new NackMethod(deliveryTag, false, requeue));
}