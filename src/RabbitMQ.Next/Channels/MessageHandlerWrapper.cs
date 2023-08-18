using System;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Channels;

internal class MessageHandlerWrapper<TMethod> : IMessageHandlerInternal
    where TMethod: struct, IIncomingMethod
{
    private readonly IMessageHandler<TMethod> wrapped;
    private readonly IMethodParser<TMethod> parser;

    public MessageHandlerWrapper(IMessageHandler<TMethod> wrapped)
    {
        this.wrapped = wrapped;
        this.parser = MethodRegistry.GetParser<TMethod>();
    }

    public void ProcessMessage(ReadOnlyMemory<byte> methodArgs, PayloadAccessor payload)
    {
        methodArgs = methodArgs[sizeof(uint)..];
        
        var args = this.parser.Parse(methodArgs.Span);
        this.wrapped.Handle(args, payload);
    }

    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }
}