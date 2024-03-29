using System.Buffers;

namespace RabbitMQ.Next.Buffers;

internal static class MemoryAccessorExtensions
{
    public static ReadOnlySequence<byte> ToSequence(this IMemoryAccessor source)
    {
        if (source == null || source.Size == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        if (source.Next == null)
        {
            return new ReadOnlySequence<byte>(source.Memory);
        }

        var first = new MemorySegment<byte>(source.Memory);
        var last = first;
        var current = source.Next;

        while (current != null)
        {
            last = last.Append(current.Memory);
            current = current.Next;
        }
            
        return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
    }
}