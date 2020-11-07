using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal struct MemoryOwner : IMemoryOwner<byte>
    {
        private readonly BufferManager manager;
        private byte[] memory;

        public MemoryOwner(BufferManager manager)
        {
            this.manager = manager;
            this.memory = manager.Rent();
        }

        public void Dispose()
        {
            if (this.memory == null)
            {
                return;
            }

            this.manager.Return(this.memory);
            this.memory = null;
        }

        public Memory<byte> Memory => this.memory;
    }
}