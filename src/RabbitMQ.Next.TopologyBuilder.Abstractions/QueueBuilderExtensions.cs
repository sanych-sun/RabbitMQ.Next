namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public static class QueueBuilderExtensions
    {
        public static IQueueBuilder WithMessageTtl(this IQueueBuilder builder, int ttl)
        {
            builder.SetArgument("x-message-ttl", ttl);
            return builder;
        }

        public static IQueueBuilder WithDeadLetterExchange(this IQueueBuilder builder, string exchange, string deadLetterRoutingKey = null)
        {
            builder.SetArgument("x-dead-letter-exchange", exchange);
            if (!string.IsNullOrEmpty(deadLetterRoutingKey))
            {
                builder.SetArgument("x-dead-letter-routing-key", deadLetterRoutingKey);
            }
            return builder;
        }

        public static IQueueBuilder WithMaxLength(this IQueueBuilder builder, int maxLength)
        {
            builder.SetArgument("x-max-length", maxLength);
            return builder;
        }

        public static IQueueBuilder WithMaxSize(this IQueueBuilder builder, int maxSize)
        {
            builder.SetArgument("x-max-length-bytes", maxSize);
            return builder;
        }

        public static IQueueBuilder WithDropOnOverflow(this IQueueBuilder builder)
        {
            builder.SetArgument("x-overflow", "drop-head");
            return builder;
        }

        public static IQueueBuilder WithRejectOnOverflow(this IQueueBuilder builder)
        {
            builder.SetArgument("x-overflow", "reject-publish");
            return builder;
        }

        public static IQueueBuilder WithMaxPriority(this IQueueBuilder builder, byte maxPriority)
        {
            builder.SetArgument("x-max-priority", maxPriority);
            return builder;
        }

        public static IQueueBuilder AsLazy(this IQueueBuilder builder)
        {
            builder.SetArgument("x-queue-mode", "lazy");
            return builder;
        }

        // TODO: add support of quorum queue attributes
        // https://www.rabbitmq.com/quorum-queues.html
    }
}