using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal interface ISocket : IDisposable
    {
        ValueTask SendAsync(ReadOnlyMemory<byte> payload);

        ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}