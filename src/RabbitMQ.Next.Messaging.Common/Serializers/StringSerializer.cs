using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport.Buffers;

namespace RabbitMQ.Next.Messaging.Common.Serializers
{
    public class StringSerializer : IMessageSerializer<string>
    {
        public void Serialize(IBufferWriter writer, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var maxLength = Encoding.UTF8.GetMaxByteCount(message.Length);
            var span = writer.GetSpan();

            if (span.Length > maxLength)
            {
                var bytesWritten = Encoding.UTF8.GetBytes(message, span);
                writer.Advance(bytesWritten);
            }
            else
            {
                var encoder = Encoding.UTF8.GetEncoder();
                var remaining = message.AsSpan();
                do
                {
                    var buffer = writer.GetSpan();
                    encoder.Convert(remaining, buffer, true, out var charsUsed, out var bytesUsed, out bool _);
                    writer.Advance(bytesUsed);
                    remaining = remaining.Slice(charsUsed);
                } while (remaining.Length > 0);
            }
        }

        public string Deserialize(ReadOnlySequence<byte> bytes)
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