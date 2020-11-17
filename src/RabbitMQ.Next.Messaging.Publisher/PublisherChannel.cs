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
using RabbitMQ.Next.Transport.Methods.Channel;

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
        private readonly ConcurrentQueue<(Message<TContent> Message, bool mandatory, bool immediate)> localQueue;
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
            this.localQueue = new ConcurrentQueue<(Message<TContent>, bool, bool)>();
            this.isCompleted = false;

            Task.Run(this.MessageSendLoop);
        }

        public async ValueTask WriteAsync(
            TContent content, string routingKey, MessageProperties properties = default,
            bool mandatory = false, bool immediate = false, CancellationToken cancellation = default)
        {
            if (this.isCompleted)
            {
                throw new ChannelClosedException();
            }

            await this.publishQueueSync.WaitAsync(cancellation);
            this.localQueue.Enqueue((new Message<TContent>(content, routingKey, properties), mandatory, immediate));
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
                    await this.SendMessageAsync(item.Message, item.mandatory, item.immediate);
                    this.publishQueueSync.Release();
                }
            }

            var ch = await this.OpenChannelAsync();
            // todo: move CloseMethod into IChannel
            await ch.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, default));

            this.channelCloseTcs.SetResult(true);
        }

        private async Task SendMessageAsync(Message<TContent> message, bool mandatory, bool immediate)
        {
            using var buffer = this.connection.BufferPool.Create();
            this.serializer.Serialize(buffer, message.Content);

            var ch = await this.OpenChannelAsync();
            await ch.SendAsync(
                new PublishMethod(this.options.Exchange, message.RoutingKey, mandatory, immediate),
                message.Properties, buffer.ToSequence());
        }

        private ValueTask<IChannel> OpenChannelAsync()
        {
            var ch = this.channel;
            if (ch == null || ch.IsClosed)
            {
                return this.DoOpenChannelAsync();
            }

            return new ValueTask<IChannel>(ch);
        }

        private async ValueTask<IChannel> DoOpenChannelAsync()
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