using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher: IPublisher
    {
        private readonly IConnection connection;
        private readonly ISerializer serializer;
        private readonly AsyncManualResetEvent connectionReady;
        private readonly IReadOnlyList<IMessageTransformer> transformers;

        public Publisher(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers)
        {
            this.connection = connection;
            this.serializer = serializer;
            this.transformers = transformers;

            this.connectionReady = new AsyncManualResetEvent();
            this.connection.StateChanged.Subscribe(this, s => s.ConnectionOnStateChanged);
        }

        public async ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellation = default)
        {
            await this.WhenReadyAsync(cancellation);

            header ??= new MessageHeader();
            this.transformers.Apply(content, header);
            
            using var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(content, bufferWriter);

            var channel = await this.connection.CreateChannelAsync();
            await channel.SendAsync(
                new PublishMethod(header.Exchange, header.RoutingKey,
                    header.Mandatory.GetValueOrDefault(), header.Immediate.GetValueOrDefault()),
                    header.Properties, bufferWriter.ToSequence());

            await channel.CloseAsync();
        }

        private ValueTask WhenReadyAsync(CancellationToken cancellation)
            => this.connectionReady.WaitAsync(cancellation);

        private ValueTask ConnectionOnStateChanged(ConnectionStateChanged e)
        {
            if (e.State == ConnectionState.Open)
            {
                this.connectionReady.Set();
            }
            else
            {
                this.connectionReady.Reset();
            }

            return default;
        }
    }
}