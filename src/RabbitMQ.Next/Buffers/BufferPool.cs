using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal class BufferPool : IBufferPool
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private readonly int bufferSize;

        public BufferPool(int bufferSize, int poolSize)
        {
            this.bufferSize = bufferSize;
            this.memoryPool = new DefaultObjectPool<MemoryBlock>(
                new ObjectPoolPolicy<MemoryBlock>(this.CreateMemoryBlock, _ => true), poolSize);
        }

        public MemoryBlock CreateMemory()
            => this.memoryPool.Get();

        private MemoryBlock CreateMemoryBlock()
        {
            return new MemoryBlock(this.memoryPool, this.bufferSize);
        }
    }
}