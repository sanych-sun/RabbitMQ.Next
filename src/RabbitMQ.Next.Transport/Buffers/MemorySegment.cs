using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    public class MemorySegment<T>: ReadOnlySequenceSegment<T>
    {
        public MemorySegment(Memory<T> memory)
        {
            this.Memory = memory;
        }

        public MemorySegment<T> Append(ArraySegment<T> segment)
            => this.Append(segment.AsMemory());

        public MemorySegment<T> Append(Memory<T> segment)
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