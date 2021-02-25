using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Publisher
{
    internal class BufferedPublisher : PublisherBase, IPublisher
    {
        private readonly SemaphoreSlim publishQueueSync;
        private readonly AsyncManualResetEvent waitToRead;
        private readonly TaskCompletionSource<bool> queueCompleteTcs;

        private readonly ConcurrentQueue<((string Exchange, string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) Message, IBufferWriter Content)> localQueue;
        private readonly PublisherChannelOptions options;
        private volatile bool isCompleted;


        public BufferedPublisher(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, PublisherChannelOptions options = null)
            : base(connection, serializer, transformers)
        {
            this.options = options ?? new PublisherChannelOptions(50);

            this.publishQueueSync = new SemaphoreSlim(this.options.LocalQueueLimit, this.options.LocalQueueLimit);

            this.queueCompleteTcs = new TaskCompletionSource<bool>();
            this.waitToRead = new AsyncManualResetEvent(true);
            this.localQueue = new ConcurrentQueue<((string Exchange, string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags), IBufferWriter)>();
            this.isCompleted = false;

            Task.Run(this.MessageSendLoop);
        }

        public async ValueTask PublishAsync<TContent>(TContent content, string exchange = null, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();
            if (this.isCompleted)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            await this.publishQueueSync.WaitAsync(cancellationToken);

            var message = this.ApplyTransformers(content, exchange, routingKey, properties, flags);
            var bufferWriter = this.Connection.BufferPool.Create();
            this.Serializer.Serialize(content, bufferWriter);

            this.localQueue.Enqueue((message, bufferWriter));
            this.waitToRead.Set();
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();

        protected override async ValueTask DisposeAsyncCore()
        {
            this.isCompleted = true;
            this.waitToRead.Set();
            await this.queueCompleteTcs.Task;
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
            }

            this.queueCompleteTcs.SetResult(true);
        }

    }
}