using System;

namespace RabbitMQ.Next.Publisher;

public static class ConnectionExtensions
{
    public static IPublisher Publisher(this IConnection connection, string exchange, Action<IPublisherBuilder> builder = null)
    {
        ArgumentNullException.ThrowIfNull(exchange);
        
        var publisherBuilder = new PublisherBuilder(exchange);
        builder?.Invoke(publisherBuilder);

        var publisher = new Publisher(
            connection,
            publisherBuilder.Exchange,
            publisherBuilder.PublisherConfirms,
            publisherBuilder.PublishMiddlewares,
            publisherBuilder.ReturnMiddlewares);

        return publisher;
    }
}