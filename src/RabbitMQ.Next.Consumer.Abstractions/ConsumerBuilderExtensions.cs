using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public static class ConsumerBuilderExtensions
{
    public static IConsumerBuilder NoAcknowledgement(this IConsumerBuilder builder)
    {
        builder.SetAcknowledgement(_ => null);
        return builder;
    }

    public static IConsumerBuilder BindToStream(this IConsumerBuilder consumer, string stream, StreamOffset offset, Action<IQueueConsumerBuilder> builder = null)
    {
        if (string.IsNullOrEmpty(stream))
        {
            throw new ArgumentNullException(nameof(stream));
        }
        
        object val = offset.Type switch
        {
            OffsetType.Next => "next",
            OffsetType.First => "first",
            OffsetType.Last => "last",
            OffsetType.Offset => offset.Argument,
            OffsetType.Timestamp => offset.Argument,
            OffsetType.Year => $"{offset.Argument}Y",
            OffsetType.Month => $"{offset.Argument}M",
            OffsetType.Day => $"{offset.Argument}D",
            OffsetType.Hour => $"{offset.Argument}h",
            OffsetType.Minute => $"{offset.Argument}m",
            OffsetType.Second => $"{offset.Argument}s",
            _ => throw new ArgumentOutOfRangeException(),
        };

        consumer.BindToQueue(stream, s =>
        {
            s.Argument("x-stream-offset", val);
            builder?.Invoke(s);
        });
        
        return consumer;
    }
    
    public static IConsumerBuilder UseConsumerMiddleware(this IConsumerBuilder builder, Func<IMessageProperties, IContentAccessor, Task> middleware)
    {
        builder.UseConsumerMiddleware(async (message, content, next) =>
        {
            await middleware.Invoke(message, content).ConfigureAwait(false);
            await next.Invoke(message, content).ConfigureAwait(false);
        });
        
        return builder;
    }
    
    public static IConsumerBuilder UseConsumerMiddleware(this IConsumerBuilder builder, Action<IMessageProperties, IContentAccessor> middleware)
    {
        builder.UseConsumerMiddleware((message, content, next) =>
        {
            middleware.Invoke(message, content);
            return next.Invoke(message, content);
        });
        
        return builder;
    }
}
