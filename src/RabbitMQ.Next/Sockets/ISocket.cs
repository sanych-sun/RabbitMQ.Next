using System;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Sockets;

internal interface ISocket : IDisposable
{
    void Send(MemoryBlock payload);

    void Receive(MemoryBlock buffer);
}