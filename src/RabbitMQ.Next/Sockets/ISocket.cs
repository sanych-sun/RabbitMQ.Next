using System;

namespace RabbitMQ.Next.Sockets
{
    internal interface ISocket : IDisposable
    {
        void Send(ReadOnlyMemory<byte> payload);

        int Receive(Memory<byte> buffer);
    }
}