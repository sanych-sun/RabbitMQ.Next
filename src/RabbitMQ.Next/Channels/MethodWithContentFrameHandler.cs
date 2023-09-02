using System;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Channels;

internal class MethodWithContentFrameHandler<TMethod> : IFrameHandler
    where TMethod: struct, IIncomingMethod
{
    private readonly IMessageHandler<TMethod> wrapped;
    private readonly IMethodParser<TMethod> parser;
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;

    private FrameType expectedFrameType = FrameType.Method;
    private MemoryBlock methodFrame;
    private MemoryBlock contentHeader;
    private long pendingContentSize;
    private MemoryBlock contentBodyHead;
    private MemoryBlock contentBodyTail;
    
    public MethodWithContentFrameHandler(IMessageHandler<TMethod> wrapped, IMethodParser<TMethod> parser, ObjectPool<MemoryBlock> memoryPool, ObjectPool<LazyMessageProperties> messagePropertiesPool)
    {
        this.wrapped = wrapped;
        this.parser = parser;
        this.memoryPool = memoryPool;
        this.messagePropertiesPool = messagePropertiesPool;
    }

    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }

    public FrameType AcceptFrame(FrameType type, MemoryBlock payload)
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
            PayloadAccessor payloadAccessor = null;

            if (this.contentHeader != null)
            {
                payloadAccessor = new PayloadAccessor(this.messagePropertiesPool, this.memoryPool, this.contentHeader, this.contentBodyHead);
            }

            var methodArgs = this.parser.Parse(((ReadOnlySpan<byte>)this.methodFrame).Read(out uint _));
            
            this.wrapped.Handle(methodArgs, payloadAccessor);

            this.memoryPool.Return(this.methodFrame);
            
            // reset state
            this.expectedFrameType = FrameType.Method;
            this.methodFrame = null;
            this.contentHeader = null;
            this.pendingContentSize = 0;
            this.contentBodyHead = null;
            this.contentBodyTail = null;
        }

        return this.expectedFrameType;
    }

    private FrameType ParseMethodFrame(MemoryBlock payload)
    {
        this.methodFrame = payload;
        return FrameType.ContentHeader;
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