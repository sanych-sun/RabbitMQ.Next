using System;
using System.Buffers;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    internal class DateTimeFormatter : IFormatter
    {
        public bool CanHandle(Type type) => type == typeof(DateTime);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is DateTime typed)
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

        private void FormatInternal(DateTime content, IBufferWriter<byte> writer)
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

        private DateTime ParseInternal(ReadOnlySequence<byte> bytes)
        {
            DateTime ParseSource(ReadOnlySpan<byte> data)
            {
                if(Utf8Parser.TryParse(bytes.First.Span, out DateTime result, out var consumed))
                {
                    if (consumed != data.Length)
                    {
                        throw new FormatException("Found some extra bytes after the content parsed.");
                    }

                    return result;
                }

                throw new FormatException("Cannot read the payload as int32.");
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