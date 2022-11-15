using System;
using RabbitMQ.Next.Buffers;

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

    public static void ReadIntoBuffer(this ISocket socket, BufferBuilder buffer, int count)
    {
        var received = 0;

        while (received < count)
        {
            var span = buffer.GetSegment(1, count - received);
            received += socket.Receive(span);
        }

        if (received > count)
        {
            throw new InvalidOperationException();
        }
    }
}