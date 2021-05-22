using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.Publisher
{
    internal abstract class PublisherBase : IAsyncDisposable
    {
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);
        private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
        private readonly IFrameHandler returnedFrameHandler;

        private IChannel channel;

        protected PublisherBase(IConnection connection, string exchange, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
        {
            this.Connection = connection;
            this.Exchange = exchange;
            this.Serializer = serializer;
            this.Transformers = transformers;
            this.returnedMessageHandlers = returnedMessageHandlers;

            var returnMethodParser = this.Connection.MethodRegistry.GetParser<ReturnMethod>();
            this.returnedFrameHandler = new ContentFrameHandler<ReturnMethod>((uint)MethodId.BasicReturn, returnMethodParser, this.HandleReturnedMessage, connection.BufferPool);
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected IConnection Connection { get; }

        protected string Exchange { get; }

        protected ISerializer Serializer { get; }

        protected IReadOnlyList<IMessageTransformer> Transformers { get; }

        protected bool IsDisposed { get; private set; }

        protected void CheckDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.returnedMessageHandlers != null)
            {
                for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
                {
                    this.returnedMessageHandlers[i].Dispose();
                }
            }

            this.IsDisposed = true;
            var ch = this.channel;
            this.channel = null;

            if (ch != null)
            {
                await ch.CloseAsync();
            }
        }

        protected async ValueTask SendMessageAsync((string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) message, IBufferWriter contentBuffer, CancellationToken cancellationToken = default)
        {
            var ch = await this.EnsureChannelOpenAsync(cancellationToken);
            await ch.SendAsync(new PublishMethod(this.Exchange, message.RoutingKey, (byte)message.PublishFlags), message.Properties, contentBuffer.ToSequence());
        }

        protected async ValueTask<IChannel> EnsureChannelOpenAsync(CancellationToken cancellationToken)
        {
            this.CheckDisposed();

            var ch = this.channel;
            if (ch == null || ch.Completion.IsCompleted)
            {
                ch = await this.OpenChannelAsync(cancellationToken);
            }

            return ch;
        }

        protected (string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) ApplyTransformers<TContent>(
            TContent content, string routingKey, IMessageProperties properties, PublishFlags publishFlags)
        {
            this.CheckDisposed();

            if (this.Transformers == null)
            {
                return (routingKey, properties, publishFlags);
            }

            var messageBuilder = new MessageBuilder(routingKey, properties, publishFlags);
            for (var i = 0; i <= this.Transformers.Count - 1; i++)
            {
                this.Transformers[i].Apply(content, messageBuilder);
            }

            return (messageBuilder.RoutingKey, messageBuilder, messageBuilder.PublishFlags);
        }

        private async ValueTask<IChannel> OpenChannelAsync(CancellationToken cancellationToken)
        {
            // TODO: implement functionality to wait for Open state unless connection is closed
            if (this.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync(cancellationToken);

            try
            {
                if (this.channel == null || this.channel.Completion.IsCompleted)
                {
                    this.channel = await this.Connection.CreateChannelAsync(new [] { this.returnedFrameHandler }, cancellationToken);
                    await this.channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.Exchange));
                }

                return this.channel;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }

        private void HandleReturnedMessage(ReturnMethod method, IMessageProperties properties, ReadOnlySequence<byte> content)
        {
            for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
            {
                var message = new ReturnedMessage(method.Exchange, method.RoutingKey, method.ReplyCode, method.ReplyText);

                if (this.returnedMessageHandlers[i].TryHandle(message, properties, new Content(this.Serializer, content)))
                {
                    break;
                }
            }
        }
    }
}