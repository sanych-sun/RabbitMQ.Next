using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;

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
            var chunks = new List<(char[] chunk, int actualsize)>();
            var totalChars = 0;

            var remaining = bytes;

            try
            {
                do
                {
                    var segment = remaining.FirstSpan;
                    var maxChars = Encoding.UTF8.GetMaxCharCount(segment.Length);

                    var chunk = ArrayPool<char>.Shared.Rent(maxChars);
                    var actualSize = decoder.GetChars(segment, chunk, remaining.IsSingleSegment);
                    chunks.Add((chunk, actualSize));
                    totalChars += actualSize;
                    remaining = remaining.Slice(0, segment.Length);
                } while (!remaining.IsEmpty);

                return string.Create(totalChars, chunks, (span, items) =>
                {
                    for (var i = 0; i < items.Count; i++)
                    {
                        var (chunk, size) = items[i];
                        chunk.AsSpan(0, size).CopyTo(span);
                        span = span.Slice(size);
                    }
                });
            }
            finally
            {
                for (var i = 0; i < chunks.Count; i++)
                {
                    ArrayPool<char>.Shared.Return(chunks[i].chunk);
                }
            }
        }
    }
}