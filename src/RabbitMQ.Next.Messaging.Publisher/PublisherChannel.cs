using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class PublisherChannel : IPublisherChannel
    {
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);
        private readonly SemaphoreSlim publishQueueSync;
        private readonly AsyncManualResetEvent waitToRead;
        private readonly TaskCompletionSource<bool> channelCloseTcs;
        private readonly IConnection connection;
        private readonly ISerializer serializer;
        private readonly IReadOnlyList<IMessageTransformer> transformers;
        private readonly ConcurrentQueue<(IBufferWriter Content, MessageHeader header)> localQueue;
        private readonly PublisherChannelOptions options;
        private IChannel channel;
        private volatile bool isCompleted;


        public PublisherChannel(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, PublisherChannelOptions options = null)
        {
            this.connection = connection;
            this.serializer = serializer;
            this.options = options ?? PublisherChannelOptions.Default;
            this.transformers = transformers;
            
            this.publishQueueSync = new SemaphoreSlim(this.options.LocalQueueLimit, this.options.LocalQueueLimit);

            this.channelCloseTcs = new TaskCompletionSource<bool>();
            this.waitToRead = new AsyncManualResetEvent(true);
            this.localQueue = new ConcurrentQueue<(IBufferWriter, MessageHeader)>();
            this.isCompleted = false;

            Task.Run(this.MessageSendLoop);
        }

        public async ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellation = default)
        {
            if (this.isCompleted)
            {
                throw new ChannelClosedException();
            }

            header ??= new MessageHeader();

            await this.publishQueueSync.WaitAsync(cancellation);
            
            this.transformers.Apply(content, header);
            var bufferWriter = this.connection.BufferPool.Create();
            this.serializer.Serialize(content, bufferWriter);
            this.localQueue.Enqueue((bufferWriter, header));
            this.waitToRead.Set();
        }

        public ValueTask DisposeAsync() => this.CompleteAsync();

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

                while (this.localQueue.TryDequeue(out var i))
                {
                    await this.UseChannelAsync(i, (ch, item)
                        => ch.SendAsync(
                            new PublishMethod(
                                item.header.Exchange, item.header.RoutingKey,
                                item.header.Mandatory ?? false, item.header.Immediate ?? false),
                                item.header.Properties, item.Content.ToSequence()));

                    i.Content.Dispose();
                    this.publishQueueSync.Release();
                }
            }

            await this.UseChannelAsync<object>(null, (ch, _) => ch.CloseAsync());

            this.channelCloseTcs.SetResult(true);
        }

        private async ValueTask UseChannelAsync<TState>(TState state, Func<IChannel, TState,Task> fn)
        {
            var ch = this.channel;
            if (ch == null || ch.IsClosed)
            {
                ch = await this.DoGetChannelAsync();
            }

            await fn(ch, state);
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