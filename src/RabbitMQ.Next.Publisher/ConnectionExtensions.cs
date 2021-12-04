using System;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher CreatePublisher(this IConnection connection, string exchange, Action<IPublisherBuilder> builder)
        {
            if (string.IsNullOrWhiteSpace(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }

            var publisherBuilder = new PublisherBuilder();
            builder?.Invoke(publisherBuilder);

            var messagePropsPool = new DefaultObjectPool<MessageBuilder>(
                new MessageBuilderPoolPolicy(),
                10);

            var publisher = new Publisher(connection, messagePropsPool, exchange,
                publisherBuilder.PublisherConfirms, SerializerFactory.Create(publisherBuilder.Serializers),
                publisherBuilder.Initializers, publisherBuilder.ReturnedMessageHandlers);

            return publisher;
        }
    }
}