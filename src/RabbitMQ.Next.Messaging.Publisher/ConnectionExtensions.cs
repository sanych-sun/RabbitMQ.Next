using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher<TContent> Publisher<TContent>(this IConnection connection, IMessageSerializer<TContent> serializer)
            => new Publisher<TContent>(connection, serializer);

        public static IPublisherChannel<TContent> PublisherChannel<TContent>(this IConnection connection, PublisherChannelOptions options, IMessageSerializer<TContent> serializer)
            => new PublisherChannel<TContent>(connection, options, serializer);
    }
}