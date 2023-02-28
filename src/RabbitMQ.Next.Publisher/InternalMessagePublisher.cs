using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class InternalMessagePublisher : IPublishMiddleware
{
    private readonly IChannel channel;
    private readonly ISerializer serializer;
    private readonly ConfirmMessageHandler confirms;
    private readonly string exchange;

    private long lastDeliveryTag;

    public InternalMessagePublisher(IChannel channel, string exchange, ISerializer serializer, ConfirmMessageHandler confirms)
    {
        this.channel = channel;
        this.exchange = exchange;
        this.serializer = serializer;
        this.confirms = confirms;
    }

    public async ValueTask InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation = default)
    {
        var flags = this.ComposePublishFlags(message);

        await this.channel.PublishAsync(
            (content, this.serializer, message),
            this.exchange, message.RoutingKey, message,
            (st, buffer) => st.serializer.Serialize(st.message, st.content, buffer),
            flags, cancellation);

        var deliveryTag = Interlocked.Increment(ref this.lastDeliveryTag);
        
        if (this.confirms != null)
        {
            var confirmed = await this.confirms.WaitForConfirmAsync((ulong)deliveryTag);
            if (!confirmed)
            {
                // todo: provide some useful info here
                throw new DeliveryFailedException();
            }
        }
    }

    private PublishFlags ComposePublishFlags(IMessageBuilder message)
    {
        var flags = PublishFlags.None;

        if (message.Immediate)
        {
            flags |= PublishFlags.Immediate;
        }

        if (message.Mandatory)
        {
            flags |= PublishFlags.Mandatory;
        }

        return flags;
    }
}