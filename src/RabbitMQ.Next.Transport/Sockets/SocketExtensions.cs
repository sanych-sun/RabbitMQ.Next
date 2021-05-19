using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal static class SocketExtensions
    {
        public static async ValueTask FillBufferAsync(this ISocket socket, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            while (buffer.Length > 0)
            {
                var received = await socket.ReceiveAsync(buffer, cancellationToken);
                buffer = buffer.Slice(received);
            }
        }

        public static async ValueTask FillBufferAsync(this ISocket socket, IBufferWriter<byte> target, int size, CancellationToken cancellationToken = default)
        {
            while (size > 0)
            {
                var buffer = target.GetMemory();
                if (buffer.Length > size)
                {
                    buffer = buffer.Slice(0, size);
                }

                var received = await socket.ReceiveAsync(buffer, cancellationToken);

                target.Advance(received);
                size -= received;
            }
        }
    }
}