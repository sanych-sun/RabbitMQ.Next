using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal class MemorySegment<T>: ReadOnlySequenceSegment<T>
    {
        public MemorySegment(ReadOnlyMemory<T> memory)
        {
            this.Memory = memory;
        }

        public MemorySegment<T> Append(ReadOnlyMemory<T> segment)
        {
            var chunk = new MemorySegment<T>(segment)
            {
                RunningIndex = this.RunningIndex + this.Memory.Length
            };

            this.Next = chunk;

            return chunk;
        }
    }
}