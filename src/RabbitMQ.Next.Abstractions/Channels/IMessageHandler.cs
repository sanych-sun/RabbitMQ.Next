using System;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Channels;

public interface IMessageHandler<in TMethod>
    where TMethod: struct, IIncomingMethod
{
    void Handle(TMethod method, IPayload payload);
    
    void Release(Exception ex = null);
}