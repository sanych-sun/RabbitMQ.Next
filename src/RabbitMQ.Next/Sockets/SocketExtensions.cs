using System;

namespace RabbitMQ.Next.Sockets
{
    internal static class SocketExtensions
    {
        public static void FillBuffer(this ISocket socket, Span<byte> buffer)
        {
            while (buffer.Length > 0)
            {
                var received = socket.Receive(buffer);
                buffer = buffer[received..];
            }
        }
    }
}