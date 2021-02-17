using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher : PublisherBase, IPublisher
    {
        public Publisher(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers)
            : base(connection, serializer, transformers)
        {
        }

        public async ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellationToken = default)
        {
            using var bufferWriter = this.Connection.BufferPool.Create();
            this.PrepareMessage(bufferWriter, content, ref header);

            await this.SendPreparedMessageAsync(header, bufferWriter, cancellationToken);
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();
    }
}