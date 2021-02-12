using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class ArrayFormatter : IFormatter
    {
        public bool CanHandle(Type type) => type == typeof(byte[]);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is byte[] lng)
            {
                this.FormatInternal(lng, writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public TContent Parse<TContent>(ReadOnlySequence<byte> bytes)
        {
            if (typeof(TContent) != typeof(byte[]))
            {
                throw new InvalidOperationException();
            }

            object result = this.ParseInternal(bytes);
            return (TContent)result;
        }

        private void FormatInternal(byte[] content, IBufferWriter<byte> writer)
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

        private byte[] ParseInternal(ReadOnlySequence<byte> bytes) => bytes.ToArray();
    }
}