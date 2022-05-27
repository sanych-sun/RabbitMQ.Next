using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
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
                ReadTimeout = 60000,
                WriteTimeout = 60000,
            };

            if (endpoint.UseSsl)
            {
                var sslStream = new SslStream(this.stream, false);
                sslStream.AuthenticateAsClient(endpoint.Host);

                this.stream = sslStream;
            }
        }

        public void Send(ReadOnlyMemory<byte> payload)
            => this.stream.Write(payload.Span);

        public void Flush() => this.stream.Flush();

        public int Receive(Span<byte> buffer)
        {
            var result = this.stream.Read(buffer);

            if (result == 0 && this.IsConnectionClosedByServer())
            {
                throw new SocketException();
            }

            return result;
        }

        private bool IsConnectionClosedByServer()
            => this.socket.Poll(1000, SelectMode.SelectRead) && this.socket.Available == 0;

        public void Dispose()
        {
            this.stream?.Dispose();
            this.socket?.Dispose();
        }
    }
}