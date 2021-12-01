using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
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
        private readonly IReadOnlyList<IFrameHandler> frameHandlers;
        private readonly string exchange;
        private readonly ISerializerFactory serializerFactory;
        private readonly IReadOnlyList<IMessageInitializer> transformers;
        private readonly ConfirmFrameHandler confirms;
        private ulong lastDeliveryTag;
        private bool isDisposed;

        private IChannel channel;

        public Publisher(
            IConnection connection,
            ObjectPool<MessageBuilder> messagePropsPool,
            string exchange,
            bool publisherConfirms,
            ISerializerFactory serializerFactory,
            IReadOnlyList<IMessageInitializer> transformers,
            IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
        {
            this.connection = connection;
            var handlers = new List<IFrameHandler>
            {
                new ReturnFrameHandler(serializerFactory, returnedMessageHandlers, connection.MethodRegistry.GetParser<ReturnMethod>()),
            };

            if (publisherConfirms)
            {
                this.confirms = new ConfirmFrameHandler(this.connection.MethodRegistry);
                handlers.Add(this.confirms);
            }

            this.frameHandlers = handlers;
            this.messagePropsPool = messagePropsPool;
            this.exchange = exchange;
            this.serializerFactory = serializerFactory;
            this.transformers = transformers;
            this.returnedMessageHandlers = returnedMessageHandlers;
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

        public async ValueTask PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
        {
            this.CheckDisposed();

            var properties = this.messagePropsPool.Get();
            this.ApplyInitializers(content, properties);
            propertiesBuilder?.Invoke(state, properties);

            var serializer = this.serializerFactory.Get(properties.ContentType);

            if (serializer == null)
            {
                throw new NotSupportedException($"Cannot resolve serializer for '{properties.ContentType}' content type.");
            }

            try
            {
                var ch = this.channel;
                if (ch == null || ch.Completion.IsCompleted)
                {
                    ch = await this.InitializeAsync(cancellation);
                }

                await ch.PublishAsync(
                    (content, serializer),
                    this.exchange, properties.RoutingKey, properties,
                    (st, buffer) => st.serializer.Serialize(st.content, buffer),
                    flags, cancellation);
            }
            finally
            {
                this.messagePropsPool.Return(properties);
            }

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
        private void ApplyInitializers<TContent>(TContent content, IMessageBuilder properties)
        {
            if (this.transformers == null)
            {
                return;
            }

            for (var i = 0; i < this.transformers.Count; i++)
            {
                this.transformers[i].Apply(content, properties);
            }
        }

        internal async ValueTask<IChannel> InitializeAsync(CancellationToken cancellationToken = default)
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
                    for (var i = 0; i < this.frameHandlers.Count; i++)
                    {
                        this.frameHandlers[0].Reset();
                    }

                    this.channel = await this.connection.OpenChannelAsync(this.frameHandlers, cancellationToken);
                    await this.channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange), cancellationToken);
                    if (this.confirms != null)
                    {
                        await this.channel.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(), cancellationToken);
                    }
                }

                return this.channel;
            }
            catch (Exception)
            {
                if (this.channel != null)
                {
                    await this.channel.CloseAsync();
                }

                throw;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }
    }
}