using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Sockets
{
    internal interface ISocket : IDisposable
    {
        void Send(ReadOnlyMemory<byte> payload);

        ValueTask SendAsync(ReadOnlyMemory<byte> payload);

        ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}