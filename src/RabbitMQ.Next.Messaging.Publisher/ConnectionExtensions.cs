using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    public static class ConnectionExtensions
    {
        public static IMessagePublisher<TLimit> MessagePublisher<TLimit>(this IConnection connection, IMessageSerializer<TLimit> serializer)
            => new MessagePublisher<TLimit>(connection, serializer);
    }
}