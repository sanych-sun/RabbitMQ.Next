using System;
using System.Buffers;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class StaticMemoryBlock : IMemoryOwner<byte>
    {
        public StaticMemoryBlock(Memory<byte> memory)
        {
            this.Memory = memory;
        }

        public void Dispose()
        {
            // do nothing here, as the instance should be reusable
        }

        public Memory<byte> Memory { get; }
    }
}