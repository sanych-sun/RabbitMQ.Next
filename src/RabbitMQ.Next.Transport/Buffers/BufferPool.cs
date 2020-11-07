using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;

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

        public void SetChunkSize(int chunkSize) => this.bufferManager.SetBufferSize(chunkSize);
    }
}