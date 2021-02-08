using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class LongFormatter : IFormatter<long>
    {
        public void Format(long content, IBufferWriter writer)
        {
            var span = writer.GetSpan(sizeof(long));
            span.Write(content);
            writer.Advance(sizeof(long));
        }

        public long Parse(ReadOnlySequence<byte> bytes)
        {
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