using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    public class StringFormatter : IFormatter
    {
        private static readonly int MinBufferSize = Encoding.UTF8.GetMaxByteCount(1);

        public bool CanHandle(Type type) => type == typeof(string);

        public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            if (content is string str)
            {
                this.FormatInternal(str, writer);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FormatInternal(string content, IBufferWriter<byte> writer)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var maxLength = Encoding.UTF8.GetMaxByteCount(content.Length);
            var span = writer.GetSpan();

            if (span.Length > maxLength)
            {
                var bytesWritten = Encoding.UTF8.GetBytes(content, span);
                writer.Advance(bytesWritten);
            }
            else
            {
                var encoder = Encoding.UTF8.GetEncoder();
                var remaining = content.AsSpan();
                do
                {
                    var buffer = writer.GetSpan(MinBufferSize);
                    encoder.Convert(remaining, buffer, true, out var charsUsed, out var bytesUsed, out bool _);
                    writer.Advance(bytesUsed);
                    remaining = remaining[charsUsed..];
                } while (remaining.Length > 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ParseInternal(ReadOnlySequence<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                return string.Empty;
            }

            if (bytes.IsSingleSegment)
            {
                return Encoding.UTF8.GetString(bytes.FirstSpan);
            }

            var decoder = Encoding.UTF8.GetDecoder();
            var chunks = new List<ArraySegment<char>>();
            var totalChars = 0;

            try
            {
                using var enumerator = new SequenceEnumerator<byte>(bytes);
                while (enumerator.MoveNext())
                {
                    var maxChars = Encoding.UTF8.GetMaxCharCount(enumerator.Current.Length);

                    var chunk = ArrayPool<char>.Shared.Rent(maxChars);
                    var actualSize = decoder.GetChars(enumerator.Current.Span, chunk, enumerator.IsLast);
                    chunks.Add(new ArraySegment<char>(chunk, 0, actualSize));
                    totalChars += actualSize;
                }

                return string.Create(totalChars, chunks, (span, items) =>
                {
                    for (var i = 0; i < items.Count; i++)
                    {
                        var chunk = items[i];
                        chunk.AsSpan().CopyTo(span);
                        span = span[chunk.Count..];
                    }
                });
            }
            finally
            {
                for (var i = 0; i < chunks.Count; i++)
                {
                    ArrayPool<char>.Shared.Return(chunks[i].Array);
                }
            }
        }
    }
}