using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    
    public static IPublisherBuilder UsePublishMiddleware(this IPublisherBuilder builder, Func<IMessageBuilder, IContentAccessor, Task> middleware)
    {
        builder.UsePublishMiddleware(async (message, content, next) =>
        {
            await middleware.Invoke(message, content).ConfigureAwait(false);
            await next.Invoke(message, content).ConfigureAwait(false);
        });
        
        return builder;
    }
    
    public static IPublisherBuilder UsePublishMiddleware(this IPublisherBuilder builder, Action<IMessageBuilder, IContentAccessor> middleware)
    {
        builder.UsePublishMiddleware(async (message, content, next) =>
        {
            middleware.Invoke(message, content);
            await next.Invoke(message, content).ConfigureAwait(false);
        });
        
        return builder;
    }
}
