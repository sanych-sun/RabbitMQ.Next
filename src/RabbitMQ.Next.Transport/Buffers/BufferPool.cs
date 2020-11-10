using RabbitMQ.Next.Abstractions;

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

        public MemoryOwner CreateMemory() => new MemoryOwner(this.bufferManager);

        public int MaxFrameSize
        {
            get => this.bufferManager.BufferSize;
            set => this.bufferManager.SetBufferSize(value);
        }
    }
}