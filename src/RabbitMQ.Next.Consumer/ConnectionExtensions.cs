using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Consumer
{
    public static class ConnectionExtensions
    {
        public static IConsumer NewConsumer(this IConnection connection, Action<IConsumerBuilder> builder)
        {
            var consumerBuilder = new ConsumerBuilder();
            builder?.Invoke(consumerBuilder);

            var formatters = FormatterSourceHelper.CombineFormatters(consumerBuilder.Formatters, consumerBuilder.FormatterSources);
            if (formatters == null)
            {
                throw new InvalidOperationException("Should configure at least one Formatter or FormatterSource");
            }

            var serializer = new Serializer(formatters);
            var consumerInitializer = new ConsumerInitializer(consumerBuilder.Queues, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount, consumerBuilder.AcknowledgerFactory == null);

            var consumer = new Consumer(connection, serializer, consumerBuilder.Handlers,
                consumerInitializer, consumerBuilder.AcknowledgerFactory,
                consumerBuilder.OnUnprocessedMessage, consumerBuilder.OnPoisonMessage);

            return consumer;
        }
    }
}