using System;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IReturnedMessageHandler : IDisposable
    {
        bool TryHandle(ReturnedMessage message, IContent content);
    }
}