using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher<TContent> : IPublisher<TContent>
    {
        private readonly IConnection connection;
        private readonly IMessageSerializer<TContent> serializer;
        private AsyncManualResetEvent connectionReady;

        public Publisher(IConnection connection, IMessageSerializer<TContent> serializer)
        {
            this.connection = connection;
            this.serializer = serializer;
        }

        public async ValueTask PublishAsync(TContent message, MessageHeader header = null, CancellationToken cancellation = default)
        {
            await this.WhenReadyAsync(cancellation);

            using var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(bufferWriter, message);

            var channel = await this.connection.CreateChannelAsync();
            await channel.SendAsync(
                new PublishMethod(header.Exchange, header.RoutingKey,
                    header.Mandatory.GetValueOrDefault(), header.Immediate.GetValueOrDefault()),
                    header.Properties, bufferWriter.ToSequence());

            await channel.CloseAsync();
        }

        private ValueTask WhenReadyAsync(CancellationToken cancellation)
        {
            if (this.connection.State == ConnectionState.Open)
            {
                return default;
            }

            if (this.connectionReady == null)
            {
                this.connectionReady ??= new AsyncManualResetEvent();
                this.connection.StateChanged.Subscribe(this, s => s.ConnectionOnStateChanged);
            }

            return this.connectionReady.WaitAsync(cancellation);
        }

        private ValueTask ConnectionOnStateChanged(ConnectionStateChanged e)
        {
            if (e.State == ConnectionState.Open)
            {
                this.connectionReady?.Set();
            }

            return default;
        }
    }
}