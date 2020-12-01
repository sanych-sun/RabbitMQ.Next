using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal struct MemoryOwner : IMemoryOwner<byte>
    {
        private readonly IBufferManager manager;
        private byte[] memory;

        public MemoryOwner(IBufferManager manager)
        {
            this.manager = manager;
            this.memory = this.manager.Rent();
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

                return this.memory;
            }
        }
    }
}