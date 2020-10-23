using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace RabbitMQ.Next.Transport.Sockets
{
    internal static class SocketExtensions
    {
        public static SocketError FillBuffer(this ISocket socket, Span<byte> buffer)
        {
            while (buffer.Length > 0)
            {
                var received = socket.Receive(buffer, out var responseCode);
                if (responseCode != SocketError.Success)
                {
                    return responseCode;
                }

                buffer = buffer.Slice(received);
            }

            return SocketError.Success;
        }

        public static SocketError Receive(this ISocket socket, PipeWriter target, int size)
        {
            while (size > 0)
            {
                var buffer = target.GetSpan();
                if (buffer.Length > size)
                {
                    buffer = buffer.Slice(0, size);
                }

                var received = socket.Receive(buffer, out var responseCode);
                if (responseCode != SocketError.Success)
                {
                    target.Complete();
                    return responseCode;
                }

                target.Advance(received);
                size -= received;
            }

            return SocketError.Success;
        }
    }
}