using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IAcknowledger : IAsyncDisposable
    {
        ValueTask AckAsync(ulong deliveryTag);

        ValueTask NackAsync(ulong deliveryTag, bool requeue);
    }
}