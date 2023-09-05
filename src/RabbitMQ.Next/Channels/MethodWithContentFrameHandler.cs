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
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;

    private FrameType expectedFrameType = FrameType.Method;
    private TMethod methodArgs;
    private IMemoryAccessor contentHeader;
    private long pendingContentSize;
    private IMemoryAccessor contentBodyHead;
    private IMemoryAccessor contentBodyTail;
    
    public MethodWithContentFrameHandler(IMessageHandler<TMethod> wrapped, IMethodParser<TMethod> parser, ObjectPool<LazyMessageProperties> messagePropertiesPool)
    {
        this.wrapped = wrapped;
        this.parser = parser;
        this.messagePropertiesPool = messagePropertiesPool;
    }

    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }

    public FrameType AcceptFrame(FrameType type, SharedMemory.MemoryAccessor payload)
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
                payloadAccessor = new PayloadAccessor(this.messagePropertiesPool, this.contentHeader, this.contentBodyHead);
            }
            
            this.wrapped.Handle(this.methodArgs, payloadAccessor);
            
            // reset state
            this.expectedFrameType = FrameType.Method;
            this.methodArgs = default;
            this.contentHeader = null;
            this.pendingContentSize = 0;
            this.contentBodyHead = null;
            this.contentBodyTail = null;
        }

        return this.expectedFrameType;
    }

    private FrameType ParseMethodFrame(SharedMemory.MemoryAccessor payload)
    {
        this.methodArgs = this.parser.Parse(payload.Span);
        return FrameType.ContentHeader;
    }

    private FrameType ParseContentHeaderFrame(SharedMemory.MemoryAccessor payload)
    {
        payload.Span[4..] // skip 2 obsolete shorts
            .Read(out ulong contentSize);

        this.pendingContentSize = (long)contentSize;
        this.contentHeader = payload.Slice(4 + sizeof(ulong)).AsRef();
        return FrameType.ContentBody;
    }

    private FrameType ParseContentBodyFrame(SharedMemory.MemoryAccessor payload)
    {
        if (this.contentBodyHead == null)
        {
            this.contentBodyHead = payload.AsRef();
            this.contentBodyTail = this.contentBodyHead;
        }
        else
        {
            this.contentBodyTail = this.contentBodyTail.Append(payload.AsRef());
        }

        this.pendingContentSize -= payload.Length;
        return (this.pendingContentSize > 0) ? FrameType.ContentBody : FrameType.None;
    }
}