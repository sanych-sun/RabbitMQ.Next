using System;
using System.Buffers;
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
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels;

internal sealed class Channel : IChannelInternal
{
    private readonly ChannelWriter<MemoryBlock> socketWriter;
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;
    private readonly ObjectPool<MessageBuilder> messageBuilderPool;
    private readonly TaskCompletionSource<bool> channelCompletion;
    private readonly Dictionary<uint, IFrameHandler> methodHandlers = new();

    private ulong lastDeliveryTag;
    
    public Channel(ChannelWriter<MemoryBlock> socketWriter, ObjectPool<MemoryBlock> memoryPool, ushort channelNumber, int frameMaxSize)
    {
        this.socketWriter = socketWriter;
        this.ChannelNumber = channelNumber;
        this.messageBuilderPool = new DefaultObjectPool<MessageBuilder>(new MessageBuilderPoolPolicy(memoryPool, channelNumber, frameMaxSize), 10);
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

    public ushort ChannelNumber { get; }

    public Task Completion => this.channelCompletion.Task;
    
    public IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
        where TMethod : struct, IIncomingMethod
    {
        var parser = MethodRegistry.GetParser<TMethod>();
        IFrameHandler frameHandler =  
            typeof(IHasContentMethod).IsAssignableFrom(typeof(TMethod))
            ? new MethodWithContentFrameHandler<TMethod>(handler, parser, this.messagePropertiesPool)
            : new MethodFrameHandler<TMethod>(handler, parser);
        
        var methodId = (uint)MethodRegistry.GetMethodId<TMethod>();
        this.methodHandlers.Add(methodId, frameHandler);

        return new Disposer(() => this.methodHandlers.Remove(methodId));
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod
    {
        this.ValidateState();
        
        var messageBuilder = this.messageBuilderPool.Get();
        MemoryBlock memory;
            
        try
        {
            messageBuilder.WriteMethodFrame(request);
            memory = messageBuilder.Complete();
        }
        finally
        {
            this.messageBuilderPool.Return(messageBuilder);   
        }
        
        return this.WriteToSocketAsync(memory, cancellation).AsTask();
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod where TResponse : struct, IIncomingMethod
    {
        var waitTask = this.WaitAsync<TResponse>(cancellation);
        await this.SendAsync(request, cancellation);
        return await waitTask;
    }

    public async Task<ulong> PublishAsync<TState>(
        TState contentBuilderState, string exchange, string routingKey,
        IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder,
        PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
    {
        this.ValidateState();
        
        var publishMethod = new PublishMethod(exchange, routingKey, (byte)flags);
        
        var messageBuilder = this.messageBuilderPool.Get();
        MemoryBlock memory;
            
        try
        {
            messageBuilder.WriteMethodFrame(publishMethod);
            messageBuilder.WriteContentFrame(contentBuilderState, properties, contentBuilder);
            memory = messageBuilder.Complete();
        }
        finally
        {
            this.messageBuilderPool.Return(messageBuilder);   
        }
        
        await this.WriteToSocketAsync(memory, cancellation);
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
            return await waitHandler.WaitTask;
        }
        finally
        {
            disposer.Dispose();    
        }
    }

    public async Task CloseAsync(Exception ex = null)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, MethodId.Unknown));
        this.TryComplete(ex);
    }

    public async Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId));
        this.TryComplete(new ChannelException(statusCode, description, failedMethodId));
    }
    
    private ValueTask WriteToSocketAsync(MemoryBlock memory, CancellationToken cancellation = default)
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