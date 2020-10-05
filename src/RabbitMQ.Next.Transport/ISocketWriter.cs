using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public interface ISocketWriter
    {
        Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellation = default);
    }
}