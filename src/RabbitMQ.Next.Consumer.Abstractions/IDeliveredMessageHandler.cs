using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IDeliveredMessageHandler : IDisposable
{
    ValueTask<bool> TryHandleAsync(DeliveredMessage message, IContent content);
}