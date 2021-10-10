using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    public static class ConnectionExtensions
    {
        public static async Task<IPublisher> CreatePublisherAsync(this IConnection connection, string exchange, Action<IPublisherBuilder> builder)
        {
            if (string.IsNullOrWhiteSpace(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }

            var publisherBuilder = new PublisherBuilder();
            builder?.Invoke(publisherBuilder);

            var publisher = new Publisher(connection, exchange,
                publisherBuilder.PublisherConfirms, publisherBuilder.SerializerFactory,
                publisherBuilder.Initializers, publisherBuilder.ReturnedMessageHandlers);
            await publisher.InitializeAsync();

            return publisher;
        }
    }
}