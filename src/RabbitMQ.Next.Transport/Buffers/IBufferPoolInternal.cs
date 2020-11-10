using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal interface IBufferPoolInternal : IBufferPool
    {
        MemoryOwner CreateMemory();

        int MaxFrameSize { get; }
    }
}