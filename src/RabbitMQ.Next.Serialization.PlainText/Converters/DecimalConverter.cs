using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public class DecimalConverter : PrimitiveTypeConverterBase<decimal>
{
    protected override bool TryFormat(decimal content, Span<byte> target, out int bytesWritten)
        => Utf8Formatter.TryFormat(content, target, out bytesWritten);

    protected override bool TryParse(ReadOnlySpan<byte> data, out decimal value, out int bytesConsumed)
        =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
}