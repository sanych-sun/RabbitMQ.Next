using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Publisher;

public static class ConnectionExtensions
{
    public static IPublisher Publisher(this IConnection connection, string exchange, Action<IPublisherBuilder> builder = null)
    {
        if (string.IsNullOrWhiteSpace(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }

        var publisherBuilder = new PublisherBuilder();
        builder?.Invoke(publisherBuilder);

        var messagePropsPool = new DefaultObjectPool<MessageBuilder>(
            new MessageBuilderPoolPolicy(),
            10);

        var publisher = new Publisher(connection, messagePropsPool, exchange,
            publisherBuilder.PublisherConfirms, 
            publisherBuilder.Initializers, publisherBuilder.ReturnedMessageHandlers);

        return publisher;
    }
}