using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IDeliveredMessageHandler : IDisposable
{
    Task<bool> TryHandleAsync(DeliveredMessage message, IContent content);
}