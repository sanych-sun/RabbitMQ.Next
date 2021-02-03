using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher<TContent> : IPublisher<TContent>
    {
        private readonly IConnection connection;
        private readonly IMessageSerializer<TContent> serializer;
        private readonly AsyncManualResetEvent connectionReady;
        private readonly IReadOnlyList<IMessageTransformer> transformers;

        public Publisher(IConnection connection, IMessageSerializer<TContent> serializer, IReadOnlyList<IMessageTransformer> transformers)
        {
            this.connection = connection;
            this.serializer = serializer;
            this.transformers = transformers;

            this.connectionReady = new AsyncManualResetEvent();
            this.connection.StateChanged.Subscribe(this, s => s.ConnectionOnStateChanged);
        }

        public async ValueTask PublishAsync(TContent content, MessageHeader header = null, CancellationToken cancellation = default)
        {
            await this.WhenReadyAsync(cancellation);
            
            this.transformers.Apply(content, header);
            
            using var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(bufferWriter, content);

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