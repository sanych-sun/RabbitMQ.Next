using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer;

public interface IAcknowledgement : IAsyncDisposable
{
    Task AckAsync(ulong deliveryTag);

    Task NackAsync(ulong deliveryTag, bool requeue);
}