using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal class MemoryPool : IMemoryPool
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private readonly int bufferSize;

        public MemoryPool(int bufferSize, int poolSize)
        {
            this.bufferSize = bufferSize;
            this.memoryPool = new DefaultObjectPool<MemoryBlock>(
                new ObjectPoolPolicy<MemoryBlock>(this.CreateMemoryBlock, _ => true), poolSize);
        }

        public MemoryBlock Get()
            => this.memoryPool.Get();

        private MemoryBlock CreateMemoryBlock() => new(this.memoryPool, this.bufferSize);
    }
}