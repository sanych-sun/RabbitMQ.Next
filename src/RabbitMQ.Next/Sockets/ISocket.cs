using System;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Sockets;

internal interface ISocket : IDisposable
{
    void Send(MemoryBlock payload);

    int Receive(byte[] buffer, int offset, int minBytes);
}