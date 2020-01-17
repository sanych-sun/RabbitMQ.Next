using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal class SocketWrapper : ISocket
    {
        private readonly Socket socket;

        public SocketWrapper(Socket socket)
        {
            this.socket = socket;
        }

        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> payload) => this.socket.SendAsync(payload, SocketFlags.None);

        public int Receive(Span<byte> buffer, out SocketError responseCode) => this.socket.Receive(buffer, SocketFlags.None, out responseCode);

        public void Dispose()
        {
            this.socket?.Dispose();
        }
    }
}