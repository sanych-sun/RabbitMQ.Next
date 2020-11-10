using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal interface IBufferPoolInternal : IBufferPool
    {
        MemoryOwner CreateMemory();

        int MaxFrameSize { get; set; }
    }
}