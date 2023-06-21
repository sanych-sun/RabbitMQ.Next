using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Tasks;

namespace RabbitMQ.Next.Publisher;

internal sealed class InternalMessagePublisher : IPublishMiddleware
{
    private readonly ISerializer serializer;
    private readonly string exchange;
    private IChannel publisherChannel;
    
    public InternalMessagePublisher(string exchange, ISerializer serializer)
    {
        this.exchange = exchange;
        this.serializer = serializer;
    }

    public ValueTask InitAsync(IChannel channel, CancellationToken cancellation)
    {
        this.publisherChannel = channel;
        return default;
    }

    public ValueTask<ulong> InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation = default)
    {
        var flags = this.ComposePublishFlags(message);

        return this.publisherChannel.PublishAsync(
            (content, this.serializer, message),
            this.exchange, message.RoutingKey, message,
            (st, buffer) => st.serializer.Serialize(st.message, st.content, buffer),
            flags, cancellation).AsValueTask();
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