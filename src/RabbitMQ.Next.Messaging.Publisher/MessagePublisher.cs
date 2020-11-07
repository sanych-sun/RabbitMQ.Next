using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;

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
        public bool IsReady => this.connection.State == ConnectionState.Open;

        public Task PublishAsync<TMessage>(string exchange, TMessage message, MessageProperties properties = default)
            where TMessage : TLimit
        {
            throw new System.NotImplementedException();
        }
        
        public ValueTask WaitForReadyAsync()
        {
            if (this.IsReady)
            {
                return default;
            }

            lock (this.sync)
            {
                if (this.IsReady)
                {
                    return default;
                }

                this.connectionReady ??= new TaskCompletionSource<bool>();

                return new ValueTask(this.connectionReady.Task);
            }
        }

        public void Dispose()
        {
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