using System;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Channels;

internal class MethodFrameHandler<TMethod> : IFrameHandler
    where TMethod: struct, IIncomingMethod
{
    private readonly IMessageHandler<TMethod> wrapped;
    private readonly IMethodParser<TMethod> parser;
    private readonly ObjectPool<MemoryBlock> memoryPool;

    public MethodFrameHandler(IMessageHandler<TMethod> wrapped, IMethodParser<TMethod> parser, ObjectPool<MemoryBlock> memoryPool)
    {
        this.wrapped = wrapped;
        this.parser = parser;
        this.memoryPool = memoryPool;
    }
    
    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }
    
    public FrameType AcceptFrame(FrameType type, MemoryBlock payload)
    {
        if (type != FrameType.Method)
        {
            throw new InvalidOperationException($"Unexpected {type} frame, when Method frame was expected");
        }

        ReadOnlySpan<byte> data = payload;
        data = data.Read(out uint _);
        
        var methodArgs = this.parser.Parse(data);
        this.memoryPool.Return(payload);
        this.wrapped.Handle(methodArgs, null);
        
        return FrameType.Method;
    }
}