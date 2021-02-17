using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;
using RabbitMQ.Next.MessagePublisher.Transformers;
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

        protected void PrepareMessage<TContent>(IBufferWriter bufferWriter, TContent content, ref MessageHeader header)
        {
            // todo: do something to avoid unnecessary object creation
            header ??= new MessageHeader();
            header.Properties.Headers ??= new Dictionary<string, object>();

            this.ApplyTransformers(content, header);
            this.Serializer.Serialize(content, bufferWriter);
        }

        protected ValueTask SendPreparedMessageAsync(MessageHeader header, IBufferWriter contentBuffer, CancellationToken cancellationToken = default)
        {
            return this.UseChannelAsync((header, contentBuffer), (ch, state)
                    => ch.SendAsync(new PublishMethod(
                        state.header.Exchange, state.header.RoutingKey, state.header.Mandatory.GetValueOrDefault(), state.header.Immediate.GetValueOrDefault()),
                        state.header.Properties, state.contentBuffer.ToSequence())
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyTransformers<TContent>(TContent content, MessageHeader header)
        {
            if (this.Transformers == null)
            {
                return;
            }

            for (var i = 0; i <= this.Transformers.Count - 1; i++)
            {
                this.Transformers[i].Apply(content, header);
            }
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