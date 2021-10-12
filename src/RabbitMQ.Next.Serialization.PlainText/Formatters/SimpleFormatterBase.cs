using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    public abstract class SimpleFormatterBase<T> : IFormatter
        where T : struct
    {
        public virtual bool CanHandle(Type type) => type == typeof(T) || type == typeof(T?);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content == null)
            {
                return;
            }

            if (content is T typed)
            {
                this.FormatInternal(typed, writer);
                return;
            }

            throw new ArgumentException(nameof(TContent));
        }

        public TContent Parse<TContent>(ReadOnlySequence<byte> bytes)
        {
            var parsed = this.ParseInternal(bytes);

            if (parsed is TContent result)
            {
                return result;
            }

            if (!parsed.HasValue && typeof(TContent) == typeof(T?))
            {
                return default;
            }

            throw new ArgumentException(nameof(TContent));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FormatInternal(T content, IBufferWriter<byte> writer)
        {
            var target = writer.GetSpan();
            if (this.TryFormatContent(content, target, out int bytesWritten))
            {
                writer.Advance(bytesWritten);
                return;
            }

            // should never be here.
            throw new OutOfMemoryException();
        }

        protected abstract bool TryFormatContent(T content, Span<byte> target, out int bytesWritten);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T? ParseInternal(ReadOnlySequence<byte> bytes)
        {
            T ParseSource(ReadOnlySpan<byte> data)
            {
                if(this.TryParseContent(data, out T result, out var consumed))
                {
                    if (consumed != data.Length)
                    {
                        throw new FormatException("Found some extra bytes after the content parsed.");
                    }

                    return result;
                }

                throw new FormatException($"Cannot read the payload as {typeof(T)}.");
            }

            if (bytes.IsEmpty)
            {
                return null;
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

        protected abstract bool TryParseContent(ReadOnlySpan<byte> data, out T value, out int bytesConsumed);
    }
}