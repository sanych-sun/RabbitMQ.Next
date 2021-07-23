using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal class BufferPool : IBufferPool
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;

        private readonly int bufferSize = 2 * 100 * 1024; // TODO: double max frame size, make it configurable

        public BufferPool()
        {
            this.memoryPool = new DefaultObjectPool<MemoryBlock>(
                new ObjectPoolPolicy<MemoryBlock>(this.CreateMemoryBlock, _ => true), 100);
        }

        public MemoryBlock CreateMemory()
            => this.memoryPool.Get();

        private MemoryBlock CreateMemoryBlock()
        {
            return new MemoryBlock(this.memoryPool, this.bufferSize);
        }
    }
}