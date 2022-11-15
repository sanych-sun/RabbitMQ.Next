using System;

namespace RabbitMQ.Next.Sockets;

internal static class SocketExtensions
{
    public static void FillBuffer(this ISocket socket, ArraySegment<byte> buffer)
    {
        var received = 0;
        while (buffer.Count > 0)
        {
            received += socket.Receive(buffer);
            buffer = buffer.Slice(received);
        }
    }
}