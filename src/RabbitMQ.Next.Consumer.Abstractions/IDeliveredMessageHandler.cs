using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IDeliveredMessageHandler : IDisposable
    {
        ValueTask<bool> TryHandleAsync(DeliveredMessage message, IContentAccessor content);
    }
}