using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Sockets
{
    internal class SocketWrapper : ISocket
    {
        private readonly Socket socket;
        private readonly Stream stream;

        public SocketWrapper(Socket socket, Endpoint endpoint)
        {
            this.socket = socket;

            this.stream = new NetworkStream(socket)
            {
                ReadTimeout = 10000,
                WriteTimeout = 10000,
            };

            if (endpoint.UseSsl)
            {
                var sslStream = new SslStream(this.stream, false);
                sslStream.AuthenticateAsClient(endpoint.Host);

                this.stream = sslStream;
            }
        }

        public void Send(ReadOnlyMemory<byte> payload)
        {
            this.stream.Write(payload.Span);
        }

        public int Receive(Memory<byte> buffer)
        {
            var result = this.stream.Read(buffer.Span);

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