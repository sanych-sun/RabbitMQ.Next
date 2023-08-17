using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public class SByteConverter: PrimitiveTypeConverterBase<sbyte>
{
    protected override bool TryFormat(sbyte content, Span<byte> target, out int bytesWritten)
        => Utf8Formatter.TryFormat(content, target, out bytesWritten);

    protected override bool TryParse(ReadOnlySpan<byte> data, out sbyte value, out int bytesConsumed)
        => Utf8Parser.TryParse(data, out value, out bytesConsumed);
}