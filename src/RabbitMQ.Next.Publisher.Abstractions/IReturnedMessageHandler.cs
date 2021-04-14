using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IReturnedMessageHandler : IDisposable
    {
        bool TryHandle(ReturnedMessage message, IMessageProperties properties, Content content);
    }
}