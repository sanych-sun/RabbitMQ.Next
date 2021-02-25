using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher
{
    public static class ConnectionExtensions
    {
        public static IPublisher NewPublisher(this IConnection connection, Action<IPublisherBuilder> builder)
        {
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
                return new Publisher(connection, serializer, publisherBuilder.Transformers);
            }

            return new BufferedPublisher(connection, serializer, publisherBuilder.Transformers, publisherBuilder.BufferSize);
        }
    }
}