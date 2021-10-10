using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    public static class ConnectionExtensions
    {
        public static IConsumer Consumer(this IConnection connection, Action<IConsumerBuilder> builder)
        {
            var consumerBuilder = new ConsumerBuilder();
            builder?.Invoke(consumerBuilder);

            var consumerInitializer = new ConsumerInitializer(consumerBuilder.Queues, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount, consumerBuilder.AcknowledgerFactory == null);

            var consumer = new Consumer(connection, consumerBuilder.SerializerFactory,
                consumerBuilder.Handlers, consumerInitializer, consumerBuilder.AcknowledgerFactory,
                consumerBuilder.OnUnprocessedMessage, consumerBuilder.OnPoisonMessage);

            return consumer;
        }
    }
}