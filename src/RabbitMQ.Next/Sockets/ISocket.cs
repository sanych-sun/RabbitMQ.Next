using System;

namespace RabbitMQ.Next.Sockets;

internal interface ISocket : IDisposable
{
    void Send(ReadOnlyMemory<byte> payload);

    void Flush();

    int Receive(Span<byte> buffer);
}