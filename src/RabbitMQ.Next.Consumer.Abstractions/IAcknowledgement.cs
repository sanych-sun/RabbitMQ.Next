using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer
{
    public interface IAcknowledgement : IAsyncDisposable
    {
        ValueTask AckAsync(ulong deliveryTag);

        ValueTask NackAsync(ulong deliveryTag, bool requeue);
    }
}