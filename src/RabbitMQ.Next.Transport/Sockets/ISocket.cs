using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal interface ISocket : IDisposable
    {
        ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellation = default);

        ValueTask SendAsync<TState>(TState state, Func<Func<ReadOnlyMemory<byte>, ValueTask>, TState, ValueTask> writer, CancellationToken cancellation = default);

        ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}