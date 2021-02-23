using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher : PublisherBase, IPublisher
    {
        public Publisher(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers)
            : base(connection, serializer, transformers)
        {
        }

        public async ValueTask PublishAsync<TContent>(TContent content, string exchange = null, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();

            var message = this.ApplyTransformers(content, exchange, routingKey, properties, flags);

            using var bufferWriter = this.Connection.BufferPool.Create();
            this.Serializer.Serialize(content, bufferWriter);

            await this.SendMessageAsync(message, bufferWriter, cancellationToken);
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();
    }
}