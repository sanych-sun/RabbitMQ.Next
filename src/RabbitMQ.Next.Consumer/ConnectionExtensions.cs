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
            var consumerBuilder = new ConsumerBuilder();
            builder?.Invoke(consumerBuilder);

            var consumer = new Consumer(connection, SerializerFactory.Create(consumerBuilder.Serializers), consumerBuilder.AcknowledgementFactory,
                consumerBuilder.Handlers, consumerBuilder.Queues, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount,
                consumerBuilder.OnUnprocessedMessage, consumerBuilder.OnPoisonMessage);

            return consumer;
        }
    }
}