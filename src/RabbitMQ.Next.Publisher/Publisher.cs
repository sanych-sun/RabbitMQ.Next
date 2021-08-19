using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class Publisher : IPublisher
    {
        private readonly ObjectPool<MessageBuilder> messagePropsPool;
        private readonly SemaphoreSlim channelOpenSync = new(1,1);
        private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
        private readonly IConnection connection;
        private readonly string exchange;
        private readonly ISerializer serializer;
        private readonly IReadOnlyList<IMessageInitializer> transformers;
        private readonly IMethodFormatter<PublishMethod> publishFormatter;
        private readonly bool publisherConfirms;
        private ConfirmFrameHandler confirms;
        private ulong lastDeliveryTag;
        private bool isDisposed;

        private IChannel channel;

        public Publisher(
            IConnection connection,
            string exchange,
            bool publisherConfirms,
            ISerializer serializer,
            IReadOnlyList<IMessageInitializer> transformers,
            IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
        {
            this.messagePropsPool = new DefaultObjectPool<MessageBuilder>(
                new MessageBuilderPoolPolicy(),
                100);

            this.connection = connection;
            this.exchange = exchange;
            this.publisherConfirms = publisherConfirms;
            this.serializer = serializer;
            this.transformers = transformers;
            this.returnedMessageHandlers = returnedMessageHandlers;

            this.publishFormatter = connection.MethodRegistry.GetFormatter<PublishMethod>();
        }

        public ValueTask DisposeAsync()
        {
            if (this.isDisposed)
            {
                return default;
            }

            if (this.returnedMessageHandlers != null)
            {
                for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
                {
                    this.returnedMessageHandlers[i].Dispose();
                }
            }

            this.isDisposed = true;
            var ch = this.channel;
            this.channel = null;

            if (ch != null)
            {
                return ch.CloseAsync();
            }

            return default;
        }

        public ValueTask PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            var properties = this.messagePropsPool.Get();
            this.ApplyInitializers(content, properties);
            propertiesBuilder?.Invoke(properties);
            return this.InternalPublishAsync(content, properties, flags);
        }

        public ValueTask PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            var properties = this.messagePropsPool.Get();
            this.ApplyInitializers(content, properties);
            propertiesBuilder?.Invoke(state, properties);
            return this.InternalPublishAsync(content, properties, flags);
        }

        private async ValueTask InternalPublishAsync<TContent>(TContent content, MessageBuilder builder, PublishFlags flags)
        {
            this.CheckDisposed();
            this.EnsureChannel();

            await this.channel.SendAsync((properties: builder, flags, publisher: this, content), (state, frameBuilder) =>
            {
                var method = new PublishMethod(state.publisher.exchange, state.properties.RoutingKey, (byte) state.flags);
                var methodBuffer = frameBuilder.BeginMethodFrame(MethodId.BasicPublish);

                var written = state.publisher.publishFormatter.Write(methodBuffer.GetMemory(), method);
                methodBuffer.Advance(written);
                frameBuilder.EndFrame();

                var bodyWriter = frameBuilder.BeginContentFrame(state.properties);
                state.publisher.serializer.Serialize(state.content, bodyWriter);
                frameBuilder.EndFrame();
            });

            this.messagePropsPool.Return(builder);
            var messageDeliveryTag = Interlocked.Increment(ref this.lastDeliveryTag);

            if (this.confirms != null)
            {
                var confirmed = await this.confirms.WaitForConfirmAsync(messageDeliveryTag);
                if (!confirmed)
                {
                    // todo: provide some useful info here
                    throw new DeliveryFailedException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureChannel()
        {
            if (this.channel.Completion.Exception != null)
            {
                throw this.channel.Completion.Exception?.InnerException ?? this.channel.Completion.Exception;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyInitializers<TContent>(TContent content, IMessageBuilder properties)
        {
            if (this.transformers == null)
            {
                return;
            }

            for (var i = 0; i <= this.transformers.Count - 1; i++)
            {
                this.transformers[i].Apply(content, properties);
            }
        }

        public async ValueTask<IChannel> InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (this.connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync(cancellationToken);

            try
            {
                if (this.channel == null)
                {
                    this.lastDeliveryTag = 0;
                    var methodHandlers = new List<IFrameHandler>
                    {
                        new ReturnFrameHandler(this.serializer, this.returnedMessageHandlers, this.connection.MethodRegistry),
                    };

                    if (this.publisherConfirms)
                    {
                        this.confirms = new ConfirmFrameHandler(this.connection.MethodRegistry);
                        methodHandlers.Add(this.confirms);
                    }

                    this.channel = await this.connection.OpenChannelAsync(methodHandlers, cancellationToken);
                    await this.channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange), cancellationToken);
                    if (this.publisherConfirms)
                    {
                        await this.channel.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(false), cancellationToken);
                    }
                }

                return this.channel;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }
    }
}