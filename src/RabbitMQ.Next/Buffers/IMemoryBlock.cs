using System;

namespace RabbitMQ.Next.Buffers
{
    internal interface IMemoryBlock
    {
        ReadOnlyMemory<byte> Memory { get; }

        bool Reset();
    }
}