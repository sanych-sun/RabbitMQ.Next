using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Consumer
{
    public static class ConnectionExtensions
    {
        public static IConsumer Consumer(this IConnection connection, Action<IConsumerBuilder> builder)
        {
            var consumerBuilder = new ConsumerBuilder(new SerializerFactory());
            builder?.Invoke(consumerBuilder);

            var consumer = new Consumer(connection, consumerBuilder.SerializerFactory, consumerBuilder.AcknowledgerFactory,
                consumerBuilder.Handlers, consumerBuilder.Queues, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount,
                consumerBuilder.OnUnprocessedMessage, consumerBuilder.OnPoisonMessage);

            return consumer;
        }
    }
}