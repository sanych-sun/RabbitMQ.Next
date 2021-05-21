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
        private readonly SemaphoreSlim writerSemaphore;
        private readonly Func<ReadOnlyMemory<byte>, ValueTask> sendData;

        public SocketWrapper(Socket socket, bool useSsl, Endpoint endpoint)
        {
            this.writerSemaphore = new SemaphoreSlim(1, 1);
            this.socket = socket;

            this.stream = new NetworkStream(socket)
            {
                ReadTimeout = 10000,
                WriteTimeout = 10000,
            };

            if (useSsl)
            {
                var sslStream = new SslStream(this.stream, false);
                sslStream.AuthenticateAsClient(endpoint.Host);

                this.stream = sslStream;
            }

            this.sendData = (bytes) => this.stream.WriteAsync(bytes);
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellation = default)
        {
            await this.writerSemaphore.WaitAsync(cancellation);
            try
            {
                await this.sendData(payload);
            }
            finally
            {
                this.writerSemaphore.Release();
            }
        }

        public async ValueTask SendAsync<TState>(TState state, Func<Func<ReadOnlyMemory<byte>, ValueTask>, TState, ValueTask> writer, CancellationToken cancellation = default)
        {
            await this.writerSemaphore.WaitAsync(cancellation);
            try
            {
                await writer.Invoke(this.sendData, state);
            }
            finally
            {
                this.writerSemaphore.Release();
            }
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