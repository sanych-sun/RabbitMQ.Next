using System;

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
            _ => throw new ArgumentOutOfRangeException()
        };

        consumer.BindToQueue(stream, s =>
        {
            s.Argument("x-stream-offset", val);
            builder?.Invoke(s);
        });
        
        return consumer;
    }
}