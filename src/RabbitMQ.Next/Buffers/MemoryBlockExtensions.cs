using System;
using System.Buffers;

namespace RabbitMQ.Next.Buffers
{
    internal static class MemoryBlockExtensions
    {
        public static ReadOnlySequence<byte> ToSequence(this MemoryBlock source)
        {
            if (source == null || source.Data.IsEmpty)
            {
                return ReadOnlySequence<byte>.Empty;
            }

            if (source.Next == null)
            {
                return new ReadOnlySequence<byte>(source.Data);
            }

            var first = new MemorySegment<byte>(source.Data);
            var last = first;
            var current = source.Next;

            while (current != null)
            {
                last = last.Append(current.Data);
                current = current.Next;
            }
            
            return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
        }

        public static void Write(this MemoryBlock memory, ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
            {
                return;
            }

            if (data.Length > memory.Memory.Length)
            {
                throw new OutOfMemoryException();
            }
            
            data.CopyTo(memory.Memory.Span);
            memory.Commit(data.Length);
        }
    }
}