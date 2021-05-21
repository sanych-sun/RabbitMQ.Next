using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher NewPublisher(this IConnection connection, string exchange, Action<IPublisherBuilder> builder)
        {
            if (string.IsNullOrWhiteSpace(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }

            var publisherBuilder = new PublisherBuilder();
            builder?.Invoke(publisherBuilder);

            var formatters = FormatterSourceHelper.CombineFormatters(publisherBuilder.Formatters, publisherBuilder.FormatterSources);
            if (formatters == null)
            {
                throw new InvalidOperationException("Should configure at least one Formatter or FormatterSource");
            }

            var serializer = new Serializer(formatters);

            if (publisherBuilder.BufferSize == 0)
            {
                return new Publisher(connection, exchange, serializer, publisherBuilder.Transformers, publisherBuilder.ReturnedMessageHandlers);
            }

            return new BufferedPublisher(connection, exchange, serializer, publisherBuilder.Transformers, publisherBuilder.ReturnedMessageHandlers, publisherBuilder.BufferSize);
        }
    }
}