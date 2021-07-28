using System;
using System.Buffers;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock : IMemoryOwner<byte>
    {
        private readonly ObjectPool<MemoryBlock> pool;
        private readonly byte[] buffer;
        private Memory<byte> memory;

        public MemoryBlock(ObjectPool<MemoryBlock> pool, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.pool = pool;
            this.buffer = new byte[size];
            this.memory = this.buffer;
        }

        public void Dispose()
        {
            this.memory = this.buffer;
            this.pool.Return(this);
        }

        public int BufferCapacity
            => this.memory.Length;

        public Memory<byte> Memory
            => this.memory;

        public Span<byte> Span
            => this.memory.Span;

        public void Slice(int length)
        {
            this.memory = this.memory.Slice(0, length);
        }
    }
}