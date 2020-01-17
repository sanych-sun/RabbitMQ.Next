using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal interface ISocket : IDisposable
    {
        ValueTask<int> SendAsync(ReadOnlyMemory<byte> payload);

        int Receive(Span<byte> buffer, out SocketError responseCode);
    }
}