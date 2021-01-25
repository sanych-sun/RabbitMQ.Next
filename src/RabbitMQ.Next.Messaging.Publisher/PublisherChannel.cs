using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class PublisherChannel<TContent> : IPublisherChannel<TContent>
    {
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);
        private readonly SemaphoreSlim publishQueueSync;
        private readonly AsyncManualResetEvent waitToRead;
        private readonly TaskCompletionSource<bool> channelCloseTcs;
        private readonly IConnection connection;
        private readonly IMessageSerializer<TContent> serializer;
        private readonly ConcurrentQueue<(TContent Payload, MessageHeader header)> localQueue;
        private readonly PublisherChannelOptions options;
        private IChannel channel;
        private volatile bool isCompleted;


        public PublisherChannel(IConnection connection, PublisherChannelOptions options, IMessageSerializer<TContent> serializer)
        {
            this.connection = connection;
            this.serializer = serializer;
            this.options = options;
            
            this.publishQueueSync = new SemaphoreSlim(this.options.localQueueLimit, this.options.localQueueLimit);

            this.channelCloseTcs = new TaskCompletionSource<bool>();
            this.waitToRead = new AsyncManualResetEvent(true);
            this.localQueue = new ConcurrentQueue<(TContent, MessageHeader)>();
            this.isCompleted = false;

            Task.Run(this.MessageSendLoop);
        }

        public async ValueTask PublishAsync(TContent message, MessageHeader header = null, CancellationToken cancellation = default)
        {
            if (this.isCompleted)
            {
                throw new ChannelClosedException();
            }

            await this.publishQueueSync.WaitAsync(cancellation);
            this.localQueue.Enqueue((message, header));
            this.waitToRead.Set();
        }

        public ValueTask CompleteAsync()
        {
            if (this.isCompleted)
            {
                return default;
            }

            this.isCompleted = true;
            return new ValueTask(this.channelCloseTcs.Task);
        }
        

        private async Task MessageSendLoop()
        {
            while (!this.isCompleted)
            {
                await this.waitToRead.WaitAsync();

                while (this.localQueue.TryDequeue(out var item))
                {
                    await this.SendMessageAsync(
                        new Message<TContent>(item.header.Exchange, item.Payload, item.header.RoutingKey, item.header.Properties),
                        item.header.Mandatory.GetValueOrDefault(), item.header.Immediate.GetValueOrDefault());
                    this.publishQueueSync.Release();
                }
            }

            var ch = await this.GetChannelAsync();
            await ch.CloseAsync();

            this.channelCloseTcs.SetResult(true);
        }

        private async Task SendMessageAsync(Message<TContent> message, bool mandatory, bool immediate)
        {
            using var buffer = this.connection.BufferPool.Create();
            this.serializer.Serialize(buffer, message.Content);

            var ch = await this.GetChannelAsync();
            await ch.SendAsync(
                new PublishMethod(message.Exchange, message.RoutingKey, mandatory, immediate),
                message.Properties, buffer.ToSequence());
        }

        private ValueTask<IChannel> GetChannelAsync()
        {
            var ch = this.channel;
            if (ch == null || ch.IsClosed)
            {
                return this.DoGetChannelAsync();
            }

            return new ValueTask<IChannel>(ch);
        }

        private async ValueTask<IChannel> DoGetChannelAsync()
        {
            if (this.connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync();

            try
            {
                this.channel ??= await this.connection.CreateChannelAsync();
                return this.channel;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }
    }
}