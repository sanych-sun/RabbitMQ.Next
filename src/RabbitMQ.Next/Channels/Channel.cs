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
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;
    private readonly ObjectPool<MessageBuilder> messageBuilderPool;
    private readonly TaskCompletionSource<bool> channelCompletion;
    private readonly Dictionary<uint, IMessageHandlerInternal> methodHandlers = new();
    
    private ulong lastDeliveryTag;

    public Channel(ChannelWriter<MemoryBlock> socketWriter, ObjectPool<MemoryBlock> memoryPool, ushort channelNumber, int frameMaxSize)
    {
        this.socketWriter = socketWriter;
        this.memoryPool = memoryPool;
        this.ChannelNumber = channelNumber;
        this.messageBuilderPool = new DefaultObjectPool<MessageBuilder>(new MessageBuilderPoolPolicy(this.memoryPool, channelNumber, frameMaxSize), 10);
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
        var methodId = (uint)MethodRegistry.GetMethodId<TMethod>();
        var wrapped = new MessageHandlerWrapper<TMethod>(handler);
        this.methodHandlers.Add(methodId, wrapped);

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
        var messageDeliveryTag = Interlocked.Increment(ref this.lastDeliveryTag);
        return messageDeliveryTag;
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
    
    
    // Incoming frame processing state
    // TODO: consider removing to specialized class
    private FrameType expectedFrameType = FrameType.Method;
    private MethodId currentMethodId = MethodId.Unknown;
    private MemoryBlock methodFrame;
    private MemoryBlock contentHeader;
    private long pendingContentSize;
    private MemoryBlock contentBodyHead;
    private MemoryBlock contentBodyTail;
    
    public void PushFrame(FrameType type, MemoryBlock payload)
    {
        if (type != this.expectedFrameType)
        {
            throw new InvalidOperationException($"Expected frame type is {this.expectedFrameType} but got {type}");
        }

        this.expectedFrameType = type switch
        {
            FrameType.Method => this.ParseMethodFrame(payload),
            FrameType.ContentHeader => this.ParseContentHeaderFrame(payload),
            FrameType.ContentBody => this.ParseContentBodyFrame(payload),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Non supported frame type: {type}"),
        };

        if (this.expectedFrameType == FrameType.None)
        {
            // do not wait for frames anymore, can execute the method
            if (this.methodHandlers.TryGetValue((uint)this.currentMethodId, out var handler))
            {
                PayloadAccessor payloadAccessor = null;

                if (this.contentHeader != null)
                {
                    payloadAccessor = new PayloadAccessor(this.messagePropertiesPool, this.memoryPool, this.contentHeader, this.contentBodyHead);
                }
                
                handler.ProcessMessage(this.methodFrame, payloadAccessor);
            }
            else
            {
                // TODO: should throw on unhandled methods?
            }

            this.memoryPool.Return(this.methodFrame);
            
            // reset state
            this.expectedFrameType = FrameType.Method;
            this.currentMethodId = MethodId.Unknown;
            this.methodFrame = null;
            this.contentHeader = null;
            this.pendingContentSize = 0;
            this.contentBodyHead = null;
            this.contentBodyTail = null;
        }
    }

    private FrameType ParseMethodFrame(MemoryBlock payload)
    {
        ((ReadOnlyMemory<byte>)payload).GetMethodId(out this.currentMethodId);
        this.methodFrame = payload;
        
        return (MethodRegistry.HasContent(this.currentMethodId)) ? FrameType.ContentHeader : FrameType.None;
    }

    private FrameType ParseContentHeaderFrame(MemoryBlock payload)
    {
        ((ReadOnlySpan<byte>)payload)[4..] // skip 2 obsolete shorts
            .Read(out ulong contentSize);

        this.pendingContentSize = (long)contentSize;
        this.contentHeader = payload;
        return FrameType.ContentBody;
    }

    private FrameType ParseContentBodyFrame(MemoryBlock payload)
    {
        if (this.contentBodyHead == null)
        {
            this.contentBodyHead = payload;
            this.contentBodyTail = payload;
        }
        else
        {
            this.contentBodyTail = this.contentBodyTail.Append(payload);
        }

        this.pendingContentSize -= payload.Length;
        return (this.pendingContentSize > 0) ? FrameType.ContentBody : FrameType.None;
    }
}