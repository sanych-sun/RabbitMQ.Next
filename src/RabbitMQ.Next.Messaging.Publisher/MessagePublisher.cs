using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class MessagePublisher<TLimit> : IMessagePublisher<TLimit>, IDisposable
    {
        private readonly IConnection connection;
        private readonly IMessageSerializer<TLimit> serializer;
        private TaskCompletionSource<bool> connectionReady;
        private readonly object sync = new object();
        
        public MessagePublisher(IConnection connection, IMessageSerializer<TLimit> serializer)
        {
            this.connection = connection;
            this.serializer = serializer;
            
            this.connection.StateChanged.Subscribe(this, s => s.ConnectionOnStateChanged);
        }

        public async Task PublishAsync<TMessage>(string exchange, TMessage message, string routingKey = null, MessageProperties properties = default)
            where TMessage : TLimit
        {
            await this.WhenReadyAsync();

            using var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(bufferWriter, message);

            var channel = await this.connection.CreateChannelAsync();
            await channel.SendAsync(
                new PublishMethod(exchange, routingKey, false, false),
                properties, bufferWriter.ToSequence());

            await channel.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, default));
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

            lock (this.sync)
            {
                this.connectionReady.SetResult(true);
                this.connectionReady = null;
            }
            
            return default;
        }
    }
}