using System;
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
    private readonly SemaphoreSlim channelOpenSync = new(1,1);
    private readonly ObjectPool<MessageBuilder> messagePropsPool;
    private readonly IConnection connection;
    private readonly string exchange;
    private readonly ISerializer serializer;
    private readonly ConfirmMessageHandler confirms;
    private readonly Func<IPublishMiddleware, IPublishMiddleware> publishPipelineFactory;
    private readonly Func<IReturnMiddleware, IReturnMiddleware> returnPipelineFactory;
    private bool isDisposed;
    private IPublishMiddleware publishPipeline;
    private IChannel channel;

    public Publisher(
        IConnection connection,
        ObjectPool<MessageBuilder> messagePropsPool,
        ISerializer serializer,
        string exchange,
        bool publisherConfirms,
        Func<IPublishMiddleware, IPublishMiddleware> publishPipelineFactory,
        Func<IReturnMiddleware, IReturnMiddleware> returnPipelineFactory)
    {
        this.connection = connection;
        this.serializer = serializer;
        if (publisherConfirms)
        {
            this.confirms = new ConfirmMessageHandler();
        }

        this.messagePropsPool = messagePropsPool;
        this.exchange = exchange;
        this.publishPipelineFactory = publishPipelineFactory;
        this.returnPipelineFactory = returnPipelineFactory;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        var ch = this.channel;
        this.channel = null;

        if (ch != null)
        {
            await ch.CloseAsync();
        }
    }

    public Task PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        propertiesBuilder?.Invoke(properties);

        return this.InternalPublishAsync(content, properties, cancellation);
    }

    public Task PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        propertiesBuilder?.Invoke(state, properties);

        return this.InternalPublishAsync(content, properties, cancellation);
    }


    private async Task InternalPublishAsync<TContent>(TContent content, MessageBuilder message, CancellationToken cancellation)
    {
        try
        {
            this.CheckDisposed();
            
            var pipeline = this.publishPipeline ?? await this.InitializeAsync(cancellation);
            await pipeline.InvokeAsync(content, message, cancellation);
        }
        finally
        {
            this.messagePropsPool.Return(message);
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

    internal async ValueTask<IPublishMiddleware> InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Connection should be in Open state to use the API");
        }

        await this.channelOpenSync.WaitAsync(cancellationToken);

        try
        {
            if (this.publishPipeline != null)
            {
                return this.publishPipeline;
            }

            this.channel = await this.connection.OpenChannelAsync(cancellationToken);
            // ensure target exchange exists
            await this.channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange), cancellationToken);

            var returnPipeline = this.returnPipelineFactory.Invoke(new VoidReturnedMiddleware());
            this.channel.WithMessageHandler(new ReturnMessageHandler(returnPipeline, this.serializer));
            if (this.confirms != null)
            {
                this.channel.WithMessageHandler<AckMethod>(this.confirms);
                this.channel.WithMessageHandler<NackMethod>(this.confirms);
            }

            if (this.confirms != null)
            {
                await this.channel.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(), cancellationToken);
            }

            var publisher = new InternalMessagePublisher(this.channel, this.exchange, this.serializer, this.confirms);

            this.publishPipeline = this.publishPipelineFactory(publisher);
            return this.publishPipeline;
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