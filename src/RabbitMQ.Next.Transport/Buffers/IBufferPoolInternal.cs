using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal interface IBufferPoolInternal : IBufferPool
    {
        MemoryOwner CreateMemory();

        int MaxFrameSize { get; set; }
    }
}