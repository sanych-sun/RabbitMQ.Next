using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class BufferedPublisher : PublisherBase, IPublisher
    {
        private readonly SemaphoreSlim publishQueueSync;
        private readonly AsyncManualResetEvent waitToRead;
        private readonly TaskCompletionSource<bool> queueCompleteTcs;

        private readonly ConcurrentQueue<(MessageHeader Header, IBufferWriter Content)> localQueue;
        private readonly PublisherChannelOptions options;
        private volatile bool isCompleted;


        public BufferedPublisher(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, PublisherChannelOptions options = null)
            : base(connection, serializer, transformers)
        {
            this.options = options;

            this.publishQueueSync = new SemaphoreSlim(this.options.LocalQueueLimit, this.options.LocalQueueLimit);

            this.queueCompleteTcs = new TaskCompletionSource<bool>();
            this.waitToRead = new AsyncManualResetEvent(true);
            this.localQueue = new ConcurrentQueue<(MessageHeader, IBufferWriter)>();
            this.isCompleted = false;

            Task.Run(this.MessageSendLoop);
        }

        public async ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellation = default)
        {
            if (this.isCompleted)
            {
                throw new ChannelClosedException();
            }

            await this.publishQueueSync.WaitAsync(cancellation);

            var bufferWriter = this.Connection.BufferPool.Create();
            this.PrepareMessage(bufferWriter, content, ref header);
            this.localQueue.Enqueue((header, bufferWriter));
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
                    await this.SendPreparedMessageAsync(i.Header, i.Content);
                    i.Content.Dispose();
                    this.publishQueueSync.Release();
                }
            }

            this.queueCompleteTcs.SetResult(true);
        }

    }
}