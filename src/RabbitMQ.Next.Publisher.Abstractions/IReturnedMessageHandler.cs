using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

public interface IReturnedMessageHandler : IDisposable
{
    Task<bool> TryHandleAsync(IReturnedMessage message);
}