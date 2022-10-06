using System;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Messaging;

namespace RabbitMQ.Next.Channels;

internal interface IMessageProcessor
{
    void Release(Exception ex);
        
    IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
        where TMethod : struct, IIncomingMethod;
        
    bool ProcessMessage(ReadOnlySpan<byte> methodArgs, PayloadAccessor payload);
}