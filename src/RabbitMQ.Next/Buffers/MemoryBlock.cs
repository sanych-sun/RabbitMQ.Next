using System;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock
    {
        private readonly byte[] buffer;
        private int offset;

        public MemoryBlock(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.buffer = new byte[size];
        }

        public bool Reset()
        {
            this.offset = 0;
            return true;
        }

        public ReadOnlyMemory<byte> Memory
            =>  new (this.buffer, 0, this.offset);

        public void Commit(int length)
        {
            this.offset += length;
        }

        public Memory<byte> Writer
            => new Memory<byte>(this.buffer).Slice(this.offset);
    }
}