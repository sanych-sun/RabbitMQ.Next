using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal class Publisher : PublisherBase, IPublisher
    {
        public Publisher(IConnection connection, string exchange, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
            : base(connection, exchange, serializer, transformers, returnedMessageHandlers)
        {
        }

        public async ValueTask PublishAsync<TContent>(TContent content, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();

            var message = this.ApplyTransformers(content, routingKey, properties, flags);

            using var bufferWriter = this.Connection.BufferPool.Create();
            this.Serializer.Serialize(content, bufferWriter);

            await this.SendMessageAsync(message, bufferWriter, cancellationToken);
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();
    }
}