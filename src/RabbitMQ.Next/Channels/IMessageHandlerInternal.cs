using System;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal interface IMessageHandlerInternal
{
    void Release(Exception ex);
        
    void ProcessMessage(ReadOnlyMemory<byte> methodArgs, PayloadAccessor payload);
}