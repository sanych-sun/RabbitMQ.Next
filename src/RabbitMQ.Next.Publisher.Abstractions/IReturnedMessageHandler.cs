using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IReturnedMessageHandler : IDisposable
    {
        ValueTask<bool> TryHandleAsync(ReturnedMessage message, IMessageProperties properties, Content content);
    }
}