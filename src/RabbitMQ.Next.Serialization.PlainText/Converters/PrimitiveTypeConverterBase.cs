using System;
using System.Buffers;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public abstract class PrimitiveTypeConverterBase<T> : IConverter<T>
    where T : struct
{
    public void Format(T content, IBufferWriter<byte> writer)
    {
        var target = writer.GetSpan();
        if (this.TryFormat(content, target, out int bytesWritten))
        {
            writer.Advance(bytesWritten);
            return;
        }

        // should never be here.
        throw new OutOfMemoryException();
    }

    public T Parse(ReadOnlySequence<byte> bytes)
    {
        T ParseSource(ReadOnlySpan<byte> data)
        {
            if(this.TryParse(data, out T result, out var consumed))
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
            throw new FormatException($"Cannot read the payload as {typeof(T)}.");
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


    protected abstract bool TryFormat(T content, Span<byte> target, out int bytesWritten);
    
    protected abstract bool TryParse(ReadOnlySpan<byte> data, out T value, out int bytesConsumed);
    
}