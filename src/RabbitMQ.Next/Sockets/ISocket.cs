using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Sockets
{
    internal interface ISocket : IDisposable
    {
        ValueTask SendAsync(ReadOnlyMemory<byte> payload);

        Task FlushAsync();

        int Receive(Span<byte> buffer);
    }
}