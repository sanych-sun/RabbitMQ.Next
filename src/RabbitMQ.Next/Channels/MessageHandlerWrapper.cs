using System;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal class MessageHandlerWrapper<TMethod> : IMessageHandlerInternal
    where TMethod: struct, IIncomingMethod
{
    private readonly IMessageHandler<TMethod> wrapped;

    public MessageHandlerWrapper(IMessageHandler<TMethod> wrapped)
    {
        this.wrapped = wrapped;
    }

    public void ProcessMessage(ReadOnlyMemory<byte> methodArgs, PayloadAccessor payload)
    {
        methodArgs = methodArgs[sizeof(uint)..];
        
        var args = methodArgs.ParseMethodArgs<TMethod>();
        this.wrapped.Handle(args, payload);
    }

    public void Release(Exception ex = null)
    {
        this.wrapped.Release(ex);
    }
}