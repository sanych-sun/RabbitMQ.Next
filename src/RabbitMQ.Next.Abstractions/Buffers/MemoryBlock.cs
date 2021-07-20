using System;
using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Buffers
{
    public struct MemoryBlock : IMemoryOwner<byte>
    {
        private readonly IBufferManager manager;
        private int size;
        private byte[] memory;

        public MemoryBlock(IBufferManager manager, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.manager = manager;
            this.memory = this.manager.Rent(size);
            this.size = size == 0 ? this.memory.Length : size;
        }

        public MemoryBlock(byte[] bytes)
        {
            this.manager = null;
            this.memory = bytes;
            this.size = bytes.Length;
        }

        public static implicit operator MemoryBlock(byte[] bytes)
            => new MemoryBlock(bytes);


        public void Dispose()
        {
            if (this.memory == null)
            {
                return;
            }

            this.manager?.Release(this.memory);
            this.memory = null;
        }

        public Memory<byte> Memory
        {
            get
            {
                if (this.memory == null)
                {
                    return Memory<byte>.Empty;
                }

                return new Memory<byte>(this.memory, 0, this.size);
            }
        }

        public void Slice(int length)
        {
            this.size = length;
        }
    }
}