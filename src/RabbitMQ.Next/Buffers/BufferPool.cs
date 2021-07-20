using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Buffers
{
    internal class BufferPool : IBufferPool
    {
        private readonly IBufferManager bufferManager;

        public BufferPool(IBufferManager bufferManager)
        {
            this.bufferManager = bufferManager;
        }

        public IBufferWriter Create()
            => new BufferWriter(this.bufferManager);

        public MemoryBlock CreateMemory(int minSize = 0)
            => new MemoryBlock(this.bufferManager, minSize);
    }
}