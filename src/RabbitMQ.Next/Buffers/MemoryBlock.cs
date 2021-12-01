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

        public int Offset => this.offset;

        public bool Reset()
        {
            this.offset = 0;
            return true;
        }

        public ReadOnlyMemory<byte> Data
            =>  new (this.buffer, 0, this.offset);

        public void Commit(int length)
        {
            this.offset += length;
        }

        public Memory<byte> Memory
            => new Memory<byte>(this.buffer)[this.offset..];

        public Span<byte> Span
            => new Span<byte>(this.buffer)[this.offset..];

        public Span<byte> Access(int start, int length)
            => new (this.buffer, start, length);
    }
}