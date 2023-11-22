using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.Publisher;

internal sealed class Publisher : IPublisher
{
    private readonly SemaphoreSlim channelInitSync = new(1, 1);
    private readonly IConnection connection;
    private readonly string exchange;
    private readonly ObjectPool<MessageBuilder> messagePropsPool;
    private readonly ConfirmMessageHandler confirms;
    private readonly ReturnMessageHandler returnMessageHandler;
    private readonly IReadOnlyList<Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task>> publishMiddlewares;
    private IChannel channel;
    private bool isDisposed;

    public Publisher(
        IConnection connection,
        string exchange,
        bool confirms,
        IReadOnlyList<Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task>> publishMiddlewares,
        IReadOnlyList<Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task>> returnMiddlewares)
    {
        this.connection = connection;
        this.exchange = exchange;
        this.messagePropsPool = new DefaultObjectPool<MessageBuilder>(
            new MessageBuilderPoolPolicy(exchange),
            10);
        if (confirms)
        {
            this.confirms = new ConfirmMessageHandler();
        }

        this.publishMiddlewares = publishMiddlewares;
        this.returnMessageHandler = new ReturnMessageHandler(returnMiddlewares);
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
            await ch.CloseAsync().ConfigureAwait(false);
        }
    }

    public Task PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        propertiesBuilder?.Invoke(properties);
        
        return this.PublishAsyncImpl(content, properties, cancellation);
    }

    public Task PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default)
    {
        var properties = this.messagePropsPool.Get();
        propertiesBuilder?.Invoke(state, properties);
        
        return this.PublishAsyncImpl(content, properties, cancellation);
    }
    
    private async Task PublishAsyncImpl<TContent>(TContent content, MessageBuilder message, CancellationToken cancellation)
    {
        try
        {
            this.CheckDisposed();
            message.SetClrType(typeof(TContent));

            if (this.publishMiddlewares?.Count > 0)
            {
                var pipeline = (IMessageBuilder m, IContentAccessor c) => this.InternalPublishAsync(m, c.Get<TContent>());
                for (var i = this.publishMiddlewares.Count - 1; i >= 0; i--)
                {
                    var next = pipeline;
                    var handler = this.publishMiddlewares[i];
                    pipeline = (m, c) => handler.Invoke(m, c, next);
                }
             
                var contentAccessor = new ContentWrapper<TContent>(content);
                await pipeline.Invoke(message, contentAccessor).ConfigureAwait(false);    
            }
            else
            {
                await this.InternalPublishAsync(message, content).ConfigureAwait(false);
            }
            
        }
        finally
        {
            this.messagePropsPool.Return(message);
        }
    }


    private async Task InternalPublishAsync<TContent>(IMessageBuilder message, TContent content)
    {
        var flags = ComposePublishFlags(message);
        var ch = await this.GetChannelAsync();
        
        var deliveryTag = await ch.PublishAsync(this.exchange, message.RoutingKey, content, message, flags);
        
        var confirmed = await this.confirms.WaitForConfirmAsync(deliveryTag, default).ConfigureAwait(false);
        if (!confirmed)
        {
            // todo: provide some useful info here
            throw new DeliveryFailedException();
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

    private ValueTask<IChannel> GetChannelAsync()
    {
        if (this.channel != null && !this.channel.Completion.IsCompleted)
        {
            return ValueTask.FromResult(this.channel);
        }

        return this.InitializeChannelAsync();
    }

    private async ValueTask<IChannel> InitializeChannelAsync()
    {
        if (this.connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Connection should be in Open state to use the Publisher API");
        }

        await this.channelInitSync.WaitAsync().ConfigureAwait(false);

        IChannel ch = null;
        try
        {
            if (this.channel != null && !this.channel.Completion.IsCompleted)
            {
                return this.channel;
            }

            ch = await this.connection.OpenChannelAsync().ConfigureAwait(false);
            
            // validate if exchange exists
            await ch.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange));

            if (this.confirms != null)
            {
                await ch.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod()).ConfigureAwait(false);
                
                ch.WithMessageHandler<AckMethod>(this.confirms);
                ch.WithMessageHandler<NackMethod>(this.confirms);
            }

            ch.WithMessageHandler(this.returnMessageHandler);

            this.channel = ch;
            return ch;
        }
        catch (Exception)
        {
            if (ch != null)
            {
                await ch.CloseAsync().ConfigureAwait(false);
            }

            throw;
        }
        finally
        {
            this.channelInitSync.Release();
        }
    }
    
    private static PublishFlags ComposePublishFlags(IMessageBuilder message)
    {
        var flags = PublishFlags.None;
        if (message.Immediate)
        {
            flags |= PublishFlags.Immediate;
        }

        if (message.Mandatory)
        {
            flags |= PublishFlags.Mandatory;
        }

        return flags;
    }
}