using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public class Int32Converter : PrimitiveTypeConverterBase<int>
{
    protected override bool TryFormat(int content, Span<byte> target, out int bytesWritten)
        => Utf8Formatter.TryFormat(content, target, out bytesWritten);

    protected override bool TryParse(ReadOnlySpan<byte> data, out int value, out int bytesConsumed)
        =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
}