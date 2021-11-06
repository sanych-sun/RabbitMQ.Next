using System;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class MemoryBlock : IMemoryBlock
    {
        private readonly byte[] buffer;
        private Memory<byte> writer;

        public MemoryBlock(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.buffer = new byte[size];
            this.writer = this.buffer;
        }

        public bool Reset()
        {
            this.writer = this.buffer;
            return true;
        }

        public ReadOnlyMemory<byte> Memory
            =>  new (this.buffer, 0, this.buffer.Length - this.writer.Length);

        public void Commit(int length)
        {
            this.writer = this.writer[length..];
        }

        public Memory<byte> Writer
            => this.writer;
    }
}