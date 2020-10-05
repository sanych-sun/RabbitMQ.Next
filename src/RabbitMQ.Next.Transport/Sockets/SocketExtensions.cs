using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal static class SocketExtensions
    {
        public static void FillBuffer(this ISocket socket, Span<byte> buffer, out SocketError responseCode)
        {
            while (buffer.Length > 0)
            {
                var received = socket.Receive(buffer, out responseCode);
                if (responseCode != SocketError.Success)
                {
                    return;
                }

                buffer = buffer.Slice(received);
            }

            responseCode = SocketError.Success;
        }

        public static void Receive(this ISocket socket, PipeWriter target, int size, out SocketError responseCode)
        {
            while (size > 0)
            {
                var buffer = target.GetSpan();
                if (buffer.Length > size)
                {
                    buffer = buffer.Slice(0, size);
                }

                var received = socket.Receive(buffer, out responseCode);
                if (responseCode != SocketError.Success)
                {
                    target.Complete();
                    return;
                }

                target.Advance(received);
                size -= received;
            }

            responseCode = SocketError.Success;
        }
    }
}