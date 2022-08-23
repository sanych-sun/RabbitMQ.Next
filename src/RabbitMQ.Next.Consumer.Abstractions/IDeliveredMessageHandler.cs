using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer;

public interface IDeliveredMessageHandler : IDisposable
{
    Task<bool> TryHandleAsync(IDeliveredMessage message);
}