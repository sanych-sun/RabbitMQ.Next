using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;

namespace RabbitMQ.Next.MessagePublisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher Publisher(this IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers = null)
            => new Publisher(connection, serializer, transformers);

        public static IPublisherChannel PublisherChannel(this IConnection connection, PublisherChannelOptions options, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers = null)
            => new PublisherChannel(connection, options, serializer, transformers);
    }
}