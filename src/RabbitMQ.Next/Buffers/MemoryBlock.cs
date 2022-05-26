using System;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock
    {
        private readonly byte[] buffer;

        public MemoryBlock(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.buffer = new byte[size];
        }

        public int Offset { get; private set; }
        
        public MemoryBlock Next { get; private set; }

        public bool Reset()
        {
            this.Offset = 0;
            this.Next = null;
            return true;
        }

        public ReadOnlyMemory<byte> Data
            =>  new (this.buffer, 0, this.Offset);

        public void Commit(int length)
        {
            this.Offset += length;
        }

        public void Rollback(int offset)
        {
            this.Offset = offset;
        }

        public MemoryBlock Append(MemoryBlock next)
        {
            this.Next = next;
            return next;
        }

        public Memory<byte> Memory
            => new Memory<byte>(this.buffer)[this.Offset..];

        public Span<byte> Span
            => new Span<byte>(this.buffer)[this.Offset..];

        public Span<byte> Access(int start, int length)
            => new (this.buffer, start, length);
    }
}