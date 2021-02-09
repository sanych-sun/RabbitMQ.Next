using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class Int32Formatter : IFormatter<int>
    {
        public void Format(int content, IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(sizeof(int));
            span.Write(content);
            writer.Advance(sizeof(int));
        }

        public int Parse(ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length != sizeof(int))
            {
                throw new ArgumentException($"Cannot parse content: expect content size {sizeof(int)} but got {bytes.Length}.");
            }

            int result;
            if (bytes.IsSingleSegment)
            {
                bytes.FirstSpan.Read(out result);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                bytes.CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out result);
            }

            return result;
        }
    }
}