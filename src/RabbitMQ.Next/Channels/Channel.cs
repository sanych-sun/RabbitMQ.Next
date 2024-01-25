using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels;

internal sealed class Channel : IChannelInternal
{
    private readonly ChannelWriter<IMemoryAccessor> socketWriter;
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;
    private readonly ObjectPool<MessageBuilder> messageBuilderPool;
    private readonly TaskCompletionSource<bool> channelCompletion;
    private readonly Dictionary<uint, IFrameHandler> methodHandlers = new();
    private readonly ISerializer serializer;

    private ulong lastDeliveryTag;
    
    public Channel(ChannelWriter<IMemoryAccessor> socketWriter, ObjectPool<MessageBuilder> messageBuilderPool, ISerializer serializer)
    {
        this.socketWriter = socketWriter;
        this.messageBuilderPool = messageBuilderPool;
        this.serializer = serializer;
        this.messagePropertiesPool = new DefaultObjectPool<LazyMessageProperties>(new LazyMessagePropertiesPolicy());

        this.channelCompletion = new TaskCompletionSource<bool>();

        var channelCloseWait = new WaitMethodMessageHandler<CloseMethod>(default);
        channelCloseWait.WaitTask.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                this.TryComplete(new ChannelException(t.Result.StatusCode, t.Result.Description, t.Result.FailedMethodId));
            }
        });

        this.WithMessageHandler(channelCloseWait);
    }
    
    public Task Completion => this.channelCompletion.Task;
    
    public IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
        where TMethod : struct, IIncomingMethod
    {
        var parser = MethodRegistry.GetParser<TMethod>();
        IFrameHandler frameHandler =  
            typeof(IHasContentMethod).IsAssignableFrom(typeof(TMethod))
            ? new MethodWithContentFrameHandler<TMethod>(this.serializer, handler, parser, this.messagePropertiesPool)
            : new MethodFrameHandler<TMethod>(handler, parser);
        
        var methodId = (uint)MethodRegistry.GetMethodId<TMethod>();
        this.methodHandlers.Add(methodId, frameHandler);

        return new Disposer(() => this.methodHandlers.Remove(methodId));
    }

    public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod
    {
        this.ValidateState();
        
        var messageBuilder = this.messageBuilderPool.Get();
        IMemoryAccessor memory;
            
        try
        {
            messageBuilder.WriteMethodFrame(request);
            memory = messageBuilder.Complete();
        }
        finally
        {
            this.messageBuilderPool.Return(messageBuilder);   
        }
        
        return this.WriteToSocketAsync(memory, cancellation);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod where TResponse : struct, IIncomingMethod
    {
        var waitTask = this.WaitAsync<TResponse>(cancellation);
        await this.SendAsync(request, cancellation).ConfigureAwait(false);
        return await waitTask.ConfigureAwait(false);
    }

    public async ValueTask<ulong> PublishAsync<TContent>(
        string exchange, 
        string routingKey,
        TContent content, 
        IMessageProperties properties,
        PublishFlags flags = PublishFlags.None,
        CancellationToken cancellation = default)
    {
        this.ValidateState();
        
        var publishMethod = new PublishMethod(exchange, routingKey, (byte)flags);
        
        var messageBuilder = this.messageBuilderPool.Get();
        IMemoryAccessor memory;
            
        try
        {
            messageBuilder.WriteMethodFrame(publishMethod);
            messageBuilder.WriteContentFrame(properties, content, this.serializer);
            memory = messageBuilder.Complete();
        }
        finally
        {
            this.messageBuilderPool.Return(messageBuilder);   
        }
        
        await this.WriteToSocketAsync(memory, cancellation).ConfigureAwait(false);
        var deliveryTag = Interlocked.Increment(ref this.lastDeliveryTag);

        return deliveryTag;
    }

    public bool TryComplete(Exception ex = null)
    {
        if (this.channelCompletion.Task.IsCompleted)
        {
            return false;
        }

        if (ex != null)
        {
            this.channelCompletion.TrySetException(ex);
        }
        else
        {
            this.channelCompletion.TrySetResult(true);
        }

        foreach (var processor in this.methodHandlers.Values)
        {
            processor.Release(ex);
        }
            
        this.methodHandlers.Clear();
            
        return true;
    }

    public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
        where TMethod : struct, IIncomingMethod
    {
        this.ValidateState();
        var waitHandler = new WaitMethodMessageHandler<TMethod>(cancellation);
        var disposer = this.WithMessageHandler(waitHandler);

        try
        {
            return await waitHandler.WaitTask.ConfigureAwait(false);
        }
        finally
        {
            disposer.Dispose();    
        }
    }

    public async Task CloseAsync(Exception ex = null)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, MethodId.Unknown)).ConfigureAwait(false);
        this.TryComplete(ex);
    }

    public async Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId)).ConfigureAwait(false);
        this.TryComplete(new ChannelException(statusCode, description, failedMethodId));
    }
    
    private ValueTask WriteToSocketAsync(IMemoryAccessor memory, CancellationToken cancellation = default)
    {
        if (this.socketWriter.TryWrite(memory))
        {
            return default;
        }

        return this.socketWriter.WriteAsync(memory, cancellation);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateState()
    {
        if (this.Completion.IsCompleted)
        {
            throw new InvalidOperationException("Cannot perform operation on closed channel");
        }
    }
    
    private IFrameHandler currentFrameHandler;
    
    public void PushFrame(FrameType type, SharedMemory.MemoryAccessor payload)
    {
        if (type == FrameType.Heartbeat)
        {
            return;
        }
        
        if (this.currentFrameHandler == null)
        {
            if (type != FrameType.Method)
            {
                throw new InvalidOperationException($"Unexpected {type} frame, when Method frame was expected");    
            }

            payload.Span.Read(out uint methodId);
            payload = payload.Slice(sizeof(uint));

            if (!this.methodHandlers.TryGetValue(methodId, out this.currentFrameHandler))
            {
                // TODO: throw channel exception here?
                throw new InvalidOperationException();
            }
        }
        
        var expectedFrame = this.currentFrameHandler.AcceptFrame(type, payload);
        if (expectedFrame == FrameType.Method)
        {
            this.currentFrameHandler = null;
        }
    }
}
