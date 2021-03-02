using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IAcknowledgement : IAsyncDisposable
    {
        ValueTask AckAsync(ulong deliveryTag, bool multiple = false);

        ValueTask NackAsync(ulong deliveryTag, bool requeue);
    }
}