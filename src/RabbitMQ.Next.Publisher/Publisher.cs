using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.Publisher;

internal sealed class Publisher : IPublisher
{
    private readonly ObjectPool<MessageBuilder> messagePropsPool;
    private readonly SemaphoreSlim channelOpenSync = new(1,1);
    private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
    private readonly IConnection connection;
    private readonly string exchange;
    private readonly ISerializerFactory serializerFactory;
    private readonly IReadOnlyList<IMessageInitializer> transformers;
    private readonly ConfirmMessageHandler confirms;
    private ulong lastDeliveryTag;
    private bool isDisposed;

    private IChannel channel;

    public Publisher(
        IConnection connection,
        ObjectPool<MessageBuilder> messagePropsPool,
        string exchange,
        bool publisherConfirms,
        IReadOnlyList<IMessageInitializer> transformers,
        IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
    {
        this.connection = connection;
        this.serializerFactory = connection.SerializerFactory;
        if (publisherConfirms)
        {
            this.confirms = new ConfirmMessageHandler();
        }

        this.messagePropsPool = messagePropsPool;
        this.exchange = exchange;
        this.transformers = transformers;
        this.returnedMessageHandlers = returnedMessageHandlers;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
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

        this.isDisposed = true;
        var ch = this.channel;
        this.channel = null;

        if (ch != null)
        {
            await ch.CloseAsync();
        }
    }

    public Task PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        this.ApplyInitializers(content, properties);
        propertiesBuilder?.Invoke(properties);

        return this.PublishAsyncInternal(content, properties, flags, cancellation);
    }

    public Task PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        this.ApplyInitializers(content, properties);
        propertiesBuilder?.Invoke(state, properties);

        return this.PublishAsyncInternal(content, properties, flags, cancellation);
    }


    private async Task PublishAsyncInternal<TContent>(TContent content, MessageBuilder message, PublishFlags flags, CancellationToken cancellation)
    {
        this.CheckDisposed();
        var serializer = this.serializerFactory.Get(message.ContentType);

        try
        {
            var ch = this.channel;
            if (ch == null || ch.Completion.IsCompleted)
            {
                ch = await this.InitializeAsync(cancellation);
            }

            await ch.PublishAsync(
                (content, serializer),
                this.exchange, message.RoutingKey, message,
                (st, buffer) => st.serializer.Serialize(st.content, buffer),
                flags, cancellation);
        }
        finally
        {
            this.messagePropsPool.Return(message);
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
        if (this.transformers == null || this.transformers.Count == 0)
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
            if (this.channel != null)
            {
                return this.channel;
            }

            this.lastDeliveryTag = 0;

            this.channel = await this.connection.OpenChannelAsync(cancellationToken);
            this.channel.WithMessageHandler(new ReturnMessageHandler(this.returnedMessageHandlers));
            if (this.confirms != null)
            {
                this.channel.WithMessageHandler<AckMethod>(this.confirms);
                this.channel.WithMessageHandler<NackMethod>(this.confirms);
            }

            await this.channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange), cancellationToken);
            if (this.confirms != null)
            {
                await this.channel.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(), cancellationToken);
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