using System;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock
    {
        private readonly byte[] buffer;
        private int commited;

        public MemoryBlock(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.buffer = new byte[size];
        }

        public int Offset => this.commited;

        public bool Reset()
        {
            this.commited = 0;
            return true;
        }

        public ReadOnlyMemory<byte> Data
            =>  new (this.buffer, 0, this.commited);

        public void Commit(int length)
        {
            this.commited += length;
        }

        public void Rollback(int offset)
        {
            this.commited = offset;
        }

        public Memory<byte> Memory
            => new Memory<byte>(this.buffer)[this.commited..];

        public Span<byte> Span
            => new Span<byte>(this.buffer)[this.commited..];

        public Span<byte> Access(int start, int length)
            => new (this.buffer, start, length);
    }
}