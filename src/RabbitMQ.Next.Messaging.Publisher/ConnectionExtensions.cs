using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;

namespace RabbitMQ.Next.MessagePublisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher<TContent> Publisher<TContent>(this IConnection connection, IMessageSerializer<TContent> serializer, IReadOnlyList<IMessageTransformer> transformers = null)
            => new Publisher<TContent>(connection, serializer, transformers);

        public static IPublisherChannel<TContent> PublisherChannel<TContent>(this IConnection connection, PublisherChannelOptions options, IMessageSerializer<TContent> serializer, IReadOnlyList<IMessageTransformer> transformers = null)
            => new PublisherChannel<TContent>(connection, options, serializer, transformers);
    }
}