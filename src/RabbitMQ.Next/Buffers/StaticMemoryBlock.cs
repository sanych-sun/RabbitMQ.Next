using System;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class StaticMemoryBlock : IMemoryBlock
    {
        public StaticMemoryBlock(ReadOnlyMemory<byte> memory)
        {
            this.Memory = memory;
        }

        public ReadOnlyMemory<byte> Memory { get; }

        public void Release()
        {
            // do nothing here, as the instance should be reusable
        }

    }
}