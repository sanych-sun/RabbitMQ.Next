using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class Int64TypeFormatter : ITypeFormatter
    {
        public bool CanHandle(Type type) => type == typeof(long);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is long lng)
            {
                this.FormatInternal(lng, writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public TContent Parse<TContent>(ReadOnlySequence<byte> bytes)
        {
            if (this.ParseInternal(bytes) is TContent result)
            {
                return result;
            }

            throw new InvalidOperationException();
        }


        private void FormatInternal(long content, IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(sizeof(long));
            span.Write(content);
            writer.Advance(sizeof(long));
        }

        private long ParseInternal(ReadOnlySequence<byte> bytes)
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