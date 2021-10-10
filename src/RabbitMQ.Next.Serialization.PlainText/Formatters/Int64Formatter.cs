using System;
using System.Buffers;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    internal class Int64Formatter : IFormatter
    {
        public bool CanHandle(Type type) => type == typeof(long);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is long typed)
            {
                this.FormatInternal(typed, writer);
                return;
            }

            throw new ArgumentException(nameof(TContent));
        }

        public TContent Parse<TContent>(ReadOnlySequence<byte> bytes)
        {
            if (this.ParseInternal(bytes) is TContent result)
            {
                return result;
            }

            throw new ArgumentException(nameof(TContent));
        }

        private void FormatInternal(long content, IBufferWriter<byte> writer)
        {
            var target = writer.GetSpan();
            if (Utf8Formatter.TryFormat(content, target, out int bytesWritten))
            {
                writer.Advance(bytesWritten);
                return;
            }

            // should never be here.
            throw new OutOfMemoryException();
        }

        private long ParseInternal(ReadOnlySequence<byte> bytes)
        {
            long ParseSource(ReadOnlySpan<byte> data)
            {
                if(Utf8Parser.TryParse(data, out long result, out var consumed))
                {
                    if (consumed != data.Length)
                    {
                        throw new FormatException("Found some extra bytes after the content parsed.");
                    }

                    return result;
                }

                throw new FormatException("Cannot read the payload as int64.");
            }

            if (bytes.IsSingleSegment)
            {
                return ParseSource(bytes.FirstSpan);
            }

            // It's unlikely to get here, but it's safer to have fallback
            Span<byte> buffer = stackalloc byte[(int)bytes.Length];
            bytes.CopyTo(buffer);

            return ParseSource(buffer);
        }
    }
}