using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IDeliveredMessageHandler : IDisposable
    {
        ValueTask<bool> TryHandleAsync(DeliveredMessage message, IMessageProperties properties, IContentAccessor content);
    }
}