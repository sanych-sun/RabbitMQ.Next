using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class Publisher<TContent> : IPublisher<TContent>, IDisposable
    {
        private readonly IConnection connection;
        private readonly IMessageSerializer<TContent> serializer;
        private TaskCompletionSource<bool> connectionReady;
        private readonly object sync = new object();
        
        public Publisher(IConnection connection, IMessageSerializer<TContent> serializer)
        {
            this.connection = connection;
            this.serializer = serializer;
            
            this.connection.StateChanged.Subscribe(this, s => s.ConnectionOnStateChanged);
        }

        public async Task PublishAsync(string exchange, TContent message, string routingKey = null, MessageProperties properties = default)
        {
            await this.WhenReadyAsync();

            using var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(bufferWriter, message);

            var channel = await this.connection.CreateChannelAsync();
            await channel.SendAsync(
                new PublishMethod(exchange, routingKey, false, false),
                properties, bufferWriter.ToSequence());

            await channel.CloseAsync();
        }

        public void Dispose()
        {
        }

        private ValueTask WhenReadyAsync()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                return default;
            }

            lock (this.sync)
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    return default;
                }

                this.connectionReady ??= new TaskCompletionSource<bool>();

                return new ValueTask(this.connectionReady.Task);
            }
        }

        private ValueTask ConnectionOnStateChanged(ConnectionStateChanged e)
        {
            if (e.State != ConnectionState.Open)
            {
                return default;
            }

            if (this.connectionReady == null)
            {
                return default;
            }

            TaskCompletionSource<bool> tcs;
            lock (this.sync)
            {
                tcs = this.connectionReady;
                this.connectionReady = null;
            }

            tcs?.SetResult(true);
            return default;
        }
    }
}