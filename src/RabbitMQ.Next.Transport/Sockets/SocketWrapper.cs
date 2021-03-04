using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal class SocketWrapper : ISocket
    {
        private readonly Socket socket;
        private readonly NetworkStream stream;

        public SocketWrapper(Socket socket)
        {
            this.socket = socket;
            this.stream = new NetworkStream(socket);
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> payload)
        {
            if (payload.IsEmpty)
            {
                return;
            }

            await this.stream.WriteAsync(payload);
        }

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => this.stream.ReadAsync(buffer, cancellationToken);

        public void Dispose()
        {
            this.stream?.Dispose();
            this.socket?.Dispose();
        }
    }
}