using System;
using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Buffers
{
    public struct MemoryOwner : IMemoryOwner<byte>
    {
        private readonly IBufferManager manager;
        private readonly int size;
        private byte[] memory;

        public MemoryOwner(IBufferManager manager, int size)
        {
            this.manager = manager;
            this.size = size;
            this.memory = this.manager.Rent(size);
        }

        public void Dispose()
        {
            if (this.memory == null)
            {
                return;
            }

            this.manager.Release(this.memory);
            this.memory = null;
        }

        public Memory<byte> Memory
        {
            get
            {
                if (this.memory == null)
                {
                    throw new ObjectDisposedException(nameof(MemoryOwner));
                }

                if (this.size == 0)
                {
                    return this.memory;
                }

                return new Memory<byte>(this.memory, 0, this.size);

            }
        }
    }
}