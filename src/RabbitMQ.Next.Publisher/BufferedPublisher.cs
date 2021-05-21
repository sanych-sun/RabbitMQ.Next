using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Publisher
{
    internal class BufferedPublisher : PublisherBase, IPublisher
    {
        private readonly SemaphoreSlim publishQueueSync;
        private readonly AsyncManualResetEvent waitToRead;
        private readonly Task messageSendTask;

        private readonly ConcurrentQueue<((string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) Message, IBufferWriter Content)> localQueue;
        private volatile bool isCompleted;


        public BufferedPublisher(IConnection connection, string exchange, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers, int bufferSize)
            : base(connection, exchange, serializer, transformers, returnedMessageHandlers)
        {
            this.publishQueueSync = new SemaphoreSlim(bufferSize);

            this.waitToRead = new AsyncManualResetEvent(true);
            this.localQueue = new ConcurrentQueue<((string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags), IBufferWriter)>();
            this.isCompleted = false;

            this.messageSendTask = this.MessageSendLoop();
        }

        public async ValueTask PublishAsync<TContent>(TContent content, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            void DisposedOrCompletedCheck()
            {
                this.CheckDisposed();
                if (this.isCompleted)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
            }

            // have to ensure channel open to validate the exchange name
            await this.EnsureChannelOpenAsync(cancellationToken);
            DisposedOrCompletedCheck();
            await this.publishQueueSync.WaitAsync(cancellationToken);
            DisposedOrCompletedCheck();

            var message = this.ApplyTransformers(content, routingKey, properties, flags);
            var bufferWriter = this.Connection.BufferPool.Create();
            this.Serializer.Serialize(content, bufferWriter);

            if (this.messageSendTask.IsCompleted)
            {
                // should never be here, but just in case the publisher was completed have to throw
                throw new ObjectDisposedException(this.GetType().Name);
            }

            this.localQueue.Enqueue((message, bufferWriter));
            this.waitToRead.Set();
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();

        protected override async ValueTask DisposeAsyncCore()
        {
            if (this.isCompleted)
            {
                return;
            }

            this.isCompleted = true;
            this.waitToRead.Set();
            await this.messageSendTask;
            this.waitToRead.Dispose();
            await base.DisposeAsyncCore();
        }

        private async Task MessageSendLoop()
        {
            while (!this.isCompleted)
            {
                await this.waitToRead.WaitAsync();

                while (this.localQueue.TryDequeue(out var i))
                {
                    await this.SendMessageAsync(i.Message, i.Content);
                    i.Content.Dispose();
                    this.publishQueueSync.Release();
                }

                this.waitToRead.Reset();
            }
        }

    }
}