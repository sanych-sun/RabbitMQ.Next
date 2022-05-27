using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public abstract class SimpleConverterBase<T> : IConverter
        where T : struct
    {
        public bool TryFormat<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is T typed)
            {
                this.FormatInternal(typed, writer);
                return true;
            }

            return content == null && typeof(TContent) == typeof(T?);
        }

        public bool TryParse<TContent>(ReadOnlySequence<byte> bytes, out TContent value)
        {
            if (typeof(TContent) == typeof(T) || typeof(TContent) == typeof(T?))
            {
                var parsed = this.ParseInternal(bytes);

                switch (parsed)
                {
                    case TContent result:
                        value = result;
                        return true;
                    case null when typeof(TContent) == typeof(T?):
                        value = default;
                        return true;
                }
            }

            value = default;
            return false;
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