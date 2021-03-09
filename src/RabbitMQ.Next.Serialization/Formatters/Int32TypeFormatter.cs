using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class Int32TypeFormatter : ITypeFormatter
    {
        public bool CanHandle(Type type) => type == typeof(int);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is int lng)
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

        private void FormatInternal(int content, IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(sizeof(int));
            span.Write(content);
            writer.Advance(sizeof(int));
        }

        private int ParseInternal(ReadOnlySequence<byte> bytes)
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