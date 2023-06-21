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

        if (publisherBuilder.Serializer == null)
        {
            throw new InvalidOperationException("Cannot create message publisher without configured serializer. Consider to call UseSerializer.");
        }

        var messagePropsPool = new DefaultObjectPool<MessageBuilder>(
            new MessageBuilderPoolPolicy(),
            10);

        var publisher = new Publisher(
            connection,
            messagePropsPool,
            publisherBuilder.Serializer,
            exchange,
            publisherBuilder.PublishMiddlewares,
            publisherBuilder.ReturnMiddlewares);

        return publisher;
    }
}