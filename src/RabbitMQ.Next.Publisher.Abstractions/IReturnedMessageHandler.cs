using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher
{
    public interface IReturnedMessageHandler : IDisposable
    {
        ValueTask<bool> TryHandleAsync(ReturnedMessage message, IContent content);
    }
}