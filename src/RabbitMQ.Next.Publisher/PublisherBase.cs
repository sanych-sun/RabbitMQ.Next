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

namespace RabbitMQ.Next.Publisher
{
    internal abstract class PublisherBase : IAsyncDisposable
    {
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);
        private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
        private readonly IFrameHandler returnedFrameHandler;

        private IChannel channel;

        protected PublisherBase(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers, IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
        {
            this.Connection = connection;
            this.Serializer = serializer;
            this.Transformers = transformers;
            this.returnedMessageHandlers = returnedMessageHandlers;

            var returnMethodParser = this.Connection.MethodRegistry.GetParser<ReturnMethod>();
            this.returnedFrameHandler = new ContentFrameHandler<ReturnMethod>((uint)MethodId.BasicReturn, returnMethodParser, this.HandleReturnedMessage, connection.BufferPool);
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected IConnection Connection { get; }

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

        protected ValueTask SendMessageAsync((string Exchange, string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) message, IBufferWriter contentBuffer, CancellationToken cancellationToken = default)
        {
            return this.UseChannelAsync(
                (message.Exchange, message.RoutingKey, message.PublishFlags, message.Properties, contentBuffer),
                (ch, state) => ch.SendAsync(
                    new PublishMethod(state.Exchange, state.RoutingKey, (byte)state.PublishFlags), state.Properties, state.contentBuffer.ToSequence())
                , cancellationToken);
        }

        protected async ValueTask UseChannelAsync<TState>(TState state, Func<IChannel, TState, Task> fn, CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();

            var ch = this.channel;
            if (ch == null || ch.IsClosed)
            {
                ch = await this.DoGetChannelAsync(cancellationToken);
            }

            await fn(ch, state);
        }

        protected (string Exchange, string RoutingKey, IMessageProperties Properties, PublishFlags PublishFlags) ApplyTransformers<TContent>(
            TContent content, string exchange, string routingKey, IMessageProperties properties, PublishFlags publishFlags)
        {
            this.CheckDisposed();

            if (this.Transformers == null)
            {
                return (exchange, routingKey, properties, publishFlags);
            }

            var messageBuilder = new MessageBuilder(exchange, routingKey, properties, publishFlags);
            for (var i = 0; i <= this.Transformers.Count - 1; i++)
            {
                this.Transformers[i].Apply(content, messageBuilder);
            }

            return (messageBuilder.Exchange, messageBuilder.RoutingKey, messageBuilder, messageBuilder.PublishFlags);
        }

        private async ValueTask<IChannel> DoGetChannelAsync(CancellationToken cancellationToken)
        {
            // TODO: implement functionality to wait for Open state unless connection is closed
            if (this.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync(cancellationToken);

            try
            {
                if (this.channel == null || this.channel.IsClosed)
                {
                    this.channel = await this.Connection.CreateChannelAsync(new [] { this.returnedFrameHandler }, cancellationToken);
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