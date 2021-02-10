using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Buffers;

namespace RabbitMQ.Next.Serialization.Formatters
{
    public class StringFormatter : IFormatter<string>
    {
        public void Format(string content, IBufferWriter<byte> writer)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var maxLength = TextEncoding.GetMaxByteCount(content.Length);
            var span = writer.GetSpan();

            if (span.Length > maxLength)
            {
                var bytesWritten = TextEncoding.GetBytes(content, span);
                writer.Advance(bytesWritten);
            }
            else
            {
                var minBufferSize = TextEncoding.GetMaxByteCount(1);
                var encoder = TextEncoding.GetEncoder();
                var remaining = content.AsSpan();
                do
                {
                    var buffer = writer.GetSpan(minBufferSize);
                    encoder.Convert(remaining, buffer, true, out var charsUsed, out var bytesUsed, out bool _);
                    writer.Advance(bytesUsed);
                    remaining = remaining.Slice(charsUsed);
                } while (remaining.Length > 0);
            }
        }

        public string Parse(ReadOnlySequence<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                return string.Empty;
            }

            if (bytes.IsSingleSegment)
            {
                return TextEncoding.GetString(bytes.FirstSpan);
            }

            var decoder = TextEncoding.GetDecoder();
            var chunks = new List<ArraySegment<char>>();
            var totalChars = 0;

            try
            {
                using var enumerator = new SequenceEnumerator<byte>(bytes);
                while (enumerator.MoveNext())
                {
                    var maxChars = TextEncoding.GetMaxCharCount(enumerator.Current.Length);

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
                        span = span.Slice(chunk.Count);
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