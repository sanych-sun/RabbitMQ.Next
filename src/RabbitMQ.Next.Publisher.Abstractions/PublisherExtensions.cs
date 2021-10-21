using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public static class PublisherExtensions
    {
        public static ValueTask PublishAsync<TContent>(this IPublisher publisher, TContent content, Action<IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
            => publisher.PublishAsync(propertiesBuilder, content, (builder, message) => builder.Invoke(message), flags, cancellation);
    }
}