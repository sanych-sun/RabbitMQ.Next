using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal abstract class PublisherBase : IAsyncDisposable
    {
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);

        private IChannel channel;
        private bool disposed;

        protected PublisherBase(IConnection connection, ISerializer serializer, IReadOnlyList<IMessageTransformer> transformers)
        {
            this.Connection = connection;
            this.Serializer = serializer;
            this.Transformers = transformers;
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected IConnection Connection { get; }

        protected ISerializer Serializer { get; }

        protected IReadOnlyList<IMessageTransformer> Transformers { get; }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.disposed)
            {
                return;
            }

            var ch = this.channel;
            if (ch == null || ch.IsClosed)
            {
                return;
            }

            this.disposed = true;
            await ch.CloseAsync();
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
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

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
            if (this.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync(cancellationToken);

            try
            {
                this.channel ??= await this.Connection.CreateChannelAsync(cancellationToken);
                return this.channel;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }
    }
}