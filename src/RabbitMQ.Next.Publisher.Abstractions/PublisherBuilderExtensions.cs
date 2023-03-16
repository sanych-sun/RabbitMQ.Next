using System;

namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    public static IPublisherBuilder UsePublishMiddleware(this IPublisherBuilder builder, Action<object, IMessageBuilder> middleware)
        => builder.UsePublishMiddleware(p => new DelegatePublishMiddleware(p, middleware));
    
    public static IPublisherBuilder UseReturnMiddleware(this IPublisherBuilder builder, Action<IReturnedMessage> middleware)
        => builder.UseReturnMiddleware(p => new DelegateReturnMiddleware(p, middleware));
}