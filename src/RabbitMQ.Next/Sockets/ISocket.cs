using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Sockets
{
    internal interface ISocket : IDisposable
    {
        ValueTask SendAsync(ReadOnlyMemory<byte> payload);

        int Receive(Memory<byte> buffer);
    }
}