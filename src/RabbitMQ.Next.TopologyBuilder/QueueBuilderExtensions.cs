namespace RabbitMQ.Next.TopologyBuilder;

public static class QueueBuilderExtensions
{
    public static IQueueBuilder WithMessageTtl(this IQueueBuilder builder, int ttl)
    {
        builder.Argument("x-message-ttl", ttl);
        return builder;
    }

    public static IQueueBuilder WithDeadLetterExchange(this IQueueBuilder builder, string exchange, string deadLetterRoutingKey = null)
    {
        builder.Argument("x-dead-letter-exchange", exchange);
        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            builder.Argument("x-dead-letter-routing-key", deadLetterRoutingKey);
        }
        return builder;
    }

    public static IQueueBuilder WithMaxLength(this IQueueBuilder builder, int maxLength)
    {
        builder.Argument("x-max-length", maxLength);
        return builder;
    }

    public static IQueueBuilder WithMaxSize(this IQueueBuilder builder, int maxSize)
    {
        builder.Argument("x-max-length-bytes", maxSize);
        return builder;
    }

    public static IQueueBuilder WithDropOnOverflow(this IQueueBuilder builder)
    {
        builder.Argument("x-overflow", "drop-head");
        return builder;
    }

    public static IQueueBuilder WithRejectOnOverflow(this IQueueBuilder builder)
    {
        builder.Argument("x-overflow", "reject-publish");
        return builder;
    }

    public static IQueueBuilder WithMaxPriority(this IQueueBuilder builder, byte maxPriority)
    {
        builder.Argument("x-max-priority", maxPriority);
        return builder;
    }

    public static IQueueBuilder AsLazy(this IQueueBuilder builder)
    {
        builder.Argument("x-queue-mode", "lazy");
        return builder;
    }
    
    public static IQueueBuilder Quorum(this IQueueBuilder builder)
    {
        builder.Argument("x-queue-type", "quorum");
        return builder;
    }
    
    public static IQueueBuilder DeliveryLimit(this IQueueBuilder builder, int deliveryLimit)
    {
        builder.Argument("x-delivery-limit", deliveryLimit);
        return builder;
    }
}