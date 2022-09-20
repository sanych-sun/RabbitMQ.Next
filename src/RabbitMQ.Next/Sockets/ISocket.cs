using System;

namespace RabbitMQ.Next.Sockets;

internal interface ISocket : IDisposable
{
    void Send(ArraySegment<byte> payload);

    void Flush();

    int Receive(ArraySegment<byte> buffer);
}