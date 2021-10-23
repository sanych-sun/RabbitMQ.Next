namespace RabbitMQ.Next.TopologyBuilder
{
    public static class QueueBuilderExtensions
    {
        public static TopologyBuilder.IQueueBuilder WithMessageTtl(this TopologyBuilder.IQueueBuilder builder, int ttl)
        {
            builder.Argument("x-message-ttl", ttl);
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithDeadLetterExchange(this TopologyBuilder.IQueueBuilder builder, string exchange, string deadLetterRoutingKey = null)
        {
            builder.Argument("x-dead-letter-exchange", exchange);
            if (!string.IsNullOrEmpty(deadLetterRoutingKey))
            {
                builder.Argument("x-dead-letter-routing-key", deadLetterRoutingKey);
            }
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithMaxLength(this TopologyBuilder.IQueueBuilder builder, int maxLength)
        {
            builder.Argument("x-max-length", maxLength);
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithMaxSize(this TopologyBuilder.IQueueBuilder builder, int maxSize)
        {
            builder.Argument("x-max-length-bytes", maxSize);
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithDropOnOverflow(this TopologyBuilder.IQueueBuilder builder)
        {
            builder.Argument("x-overflow", "drop-head");
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithRejectOnOverflow(this TopologyBuilder.IQueueBuilder builder)
        {
            builder.Argument("x-overflow", "reject-publish");
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder WithMaxPriority(this TopologyBuilder.IQueueBuilder builder, byte maxPriority)
        {
            builder.Argument("x-max-priority", maxPriority);
            return builder;
        }

        public static TopologyBuilder.IQueueBuilder AsLazy(this TopologyBuilder.IQueueBuilder builder)
        {
            builder.Argument("x-queue-mode", "lazy");
            return builder;
        }

        // TODO: add support of quorum queue attributes
        // https://www.rabbitmq.com/quorum-queues.html
    }
}