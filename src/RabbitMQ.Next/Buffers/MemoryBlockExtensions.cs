using System;
using System.Buffers;

namespace RabbitMQ.Next.Buffers;

internal static class MemoryBlockExtensions
{
    public static ReadOnlySequence<byte> ToSequence(this MemoryBlock source)
    {
        if (source == null || source.Length == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        if (source.Next == null)
        {
            return new ReadOnlySequence<byte>(source);
        }

        var first = new MemorySegment<byte>(source);
        var last = first;
        var current = source.Next;

        while (current != null)
        {
            last = last.Append(current);
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

        if (data.Length > memory.Buffer.Length)
        {
            throw new OutOfMemoryException();
        }
            
        data.CopyTo(memory.Buffer);
        memory.Slice(0, data.Length);
    }
}