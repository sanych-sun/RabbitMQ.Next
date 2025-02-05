using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Sockets;

internal interface ISocket : IDisposable
{
    Task FlushAsync();
    
    ValueTask SendAsync(ReadOnlyMemory<byte> payload);

    ValueTask<int> ReceiveAsync(Memory<byte> buffer, int minBytes);
}
