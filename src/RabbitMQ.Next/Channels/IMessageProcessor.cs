using System;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Messaging;

namespace RabbitMQ.Next.Channels
{
    internal interface IMessageProcessor : IDisposable
    {
        IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
            where TMethod : struct, IIncomingMethod;
        
        bool ProcessMessage(ReadOnlySpan<byte> methodArgs, ContentAccessor content);
    }
}