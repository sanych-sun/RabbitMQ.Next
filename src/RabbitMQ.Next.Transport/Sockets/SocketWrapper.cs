using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal class SocketWrapper : ISocket
    {
        private readonly Socket socket;
        private readonly Stream stream;

        public SocketWrapper(Socket socket, bool useSsl, Endpoint endpoint)
        {
            this.socket = socket;
            var netStream = new NetworkStream(socket);
            netStream.ReadTimeout = 1000;
            netStream.WriteTimeout = 1000;

            this.stream = netStream;
            if (useSsl)
            {
                var sslStream = new SslStream(this.stream, false);
                sslStream.AuthenticateAsClient(endpoint.Host);

                this.stream = sslStream;
            }
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> payload)
        {
            if (payload.IsEmpty)
            {
                return;
            }

            await this.stream.WriteAsync(payload);
        }

        public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var result = await this.stream.ReadAsync(buffer, cancellationToken);

            if (result == 0 && this.IsConnectionClosedByServer())
            {
                throw new SocketException();
            }

            return result;
        }

        private bool IsConnectionClosedByServer()
        {
            return this.socket.Poll(1000, SelectMode.SelectRead) && this.socket.Available == 0;
        }

        public void Dispose()
        {
            this.stream?.Dispose();
            this.socket?.Dispose();
        }
    }
}