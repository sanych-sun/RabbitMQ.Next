using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    public class MemorySegment<T>: ReadOnlySequenceSegment<T>
    {
        public MemorySegment(ArraySegment<T> segment)
        {
            this.Memory = segment.AsMemory();
        }

        public MemorySegment<T> Append(ArraySegment<T> segment)
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