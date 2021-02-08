using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class ArrayFormatter : IFormatter<byte[]>
    {
        public void Format(byte[] content, IBufferWriter writer)
        {
            ReadOnlySpan<byte> source = content;

            do
            {
                var target = writer.GetSpan();
                var chunk = source;
                if (source.Length > target.Length)
                {
                    chunk = source.Slice(0, target.Length);
                }

                chunk.CopyTo(target);
                writer.Advance(chunk.Length);
                source = source.Slice(chunk.Length);

            } while (source.Length > 0);
        }

        public byte[] Parse(ReadOnlySequence<byte> bytes) => bytes.ToArray();
    }
}