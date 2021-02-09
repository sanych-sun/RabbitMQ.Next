using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class Int64Formatter : IFormatter<long>
    {
        public void Format(long content, IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(sizeof(long));
            span.Write(content);
            writer.Advance(sizeof(long));
        }

        public long Parse(ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length != sizeof(long))
            {
                throw new ArgumentException($"Cannot parse content: expect content size {sizeof(int)} but got {bytes.Length}.");
            }

            long result;
            if (bytes.IsSingleSegment)
            {
                bytes.FirstSpan.Read(out result);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(long)];
                bytes.CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out result);
            }

            return result;
        }
    }
}