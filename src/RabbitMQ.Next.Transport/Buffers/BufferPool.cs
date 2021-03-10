using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal class BufferPool : IBufferPoolInternal
    {
        private readonly BufferManager bufferManager;

        public BufferPool(int chunkSize)
        {
            this.bufferManager = new BufferManager(chunkSize);
        }

        public IBufferWriter Create() => new BufferWriter(this.bufferManager);

        public MemoryOwner CreateMemory(int size = 0) => new MemoryOwner(this.bufferManager, size);

        public void SetBufferSize(int maxSize)
            => this.bufferManager.SetBufferSize(maxSize);
    }
}