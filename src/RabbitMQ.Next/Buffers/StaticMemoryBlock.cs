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

        // do nothing here, as the instance should be reusable
        public bool Reset() => false;

        public static implicit operator StaticMemoryBlock(byte[] data)
            => new StaticMemoryBlock(data);

        public static implicit operator StaticMemoryBlock(ReadOnlyMemory<byte> data)
            => new StaticMemoryBlock(data);
    }
}