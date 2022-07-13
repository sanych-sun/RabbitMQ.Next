using System;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Channels;

public interface IMessageHandler<in TMethod>
    where TMethod: struct, IIncomingMethod
{
    bool Handle(TMethod method, IContent content);
        
    void Release(Exception ex = null);
}