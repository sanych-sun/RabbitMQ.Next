using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;

namespace RabbitMQ.Next.MessagePublisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher Publisher(this IConnection connection,
            ISerializer serializer,
            IReadOnlyList<IMessageTransformer> transformers = null,
            PublisherChannelOptions options = null)
        {
            if (options == null || options.LocalQueueLimit <= 1)
            {
                return new Publisher(connection, serializer, transformers);
            }

            return new BufferedPublisher(connection, serializer, transformers, options);
        }
    }
}