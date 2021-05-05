using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    public interface IAcknowledgement : IAsyncDisposable
    {
        Task AckAsync(ulong deliveryTag, bool multiple = false);

        Task NackAsync(ulong deliveryTag, bool requeue);
    }
}