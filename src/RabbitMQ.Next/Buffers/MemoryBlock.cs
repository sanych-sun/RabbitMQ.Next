using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock : IMemoryBlock
    {
        private readonly ObjectPool<MemoryBlock> pool;
        private readonly byte[] buffer;
        private Memory<byte> writer;

        public MemoryBlock(ObjectPool<MemoryBlock> pool, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.pool = pool;
            this.buffer = new byte[size];
            this.writer = this.buffer;
        }

        public void Release()
        {
            this.writer = this.buffer;
            this.pool.Return(this);
        }

        public ReadOnlyMemory<byte> Memory
            =>  new ReadOnlyMemory<byte>(this.buffer, 0, this.buffer.Length - this.writer.Length);

        public void Commit(int length)
        {
            this.writer = this.writer[length..];
        }

        public Memory<byte> Writer
            => this.writer;
    }
}