using System;
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

    public MethodFrameHandler(IMessageHandler<TMethod> wrapped, IMethodParser<TMethod> parser)
    {
        this.wrapped = wrapped;
        this.parser = parser;
    }
    
    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }
    
    public FrameType AcceptFrame(FrameType type, SharedMemory.MemoryAccessor payload)
    {
        if (type != FrameType.Method)
        {
            throw new InvalidOperationException($"Unexpected {type} frame, when Method frame was expected");
        }
        
        var methodArgs = this.parser.Parse(payload.Span);
        this.wrapped.Handle(methodArgs, null);
        
        return FrameType.Method;
    }
}