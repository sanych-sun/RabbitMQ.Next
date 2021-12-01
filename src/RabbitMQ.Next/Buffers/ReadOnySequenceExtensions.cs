using System.Buffers;
using System.Collections.Generic;

namespace RabbitMQ.Next.Buffers
{
    internal static class ReadOnySequenceExtensions
    {
        public static ReadOnlySequence<byte> ToSequence(this IReadOnlyList<MemoryBlock> source)
        {
            if (source == null || source.Count == 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }

            if (source.Count == 1)
            {
                return new ReadOnlySequence<byte>(source[0].Data);
            }

            var first = new MemorySegment<byte>(source[0].Data);
            var last = first;

            for (var i = 1; i < source.Count; i++)
            {
                last = last.Append(source[i].Data);
            }

            return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

        }
    }
}