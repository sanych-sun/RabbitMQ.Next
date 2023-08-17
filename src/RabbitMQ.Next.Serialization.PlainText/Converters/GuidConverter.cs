using System;
using System.Buffers;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public class GuidConverter: PrimitiveTypeConverterBase<Guid>
{
    protected override bool TryFormat(Guid content, Span<byte> target, out int bytesWritten)
        => Utf8Formatter.TryFormat(content, target, out bytesWritten, new StandardFormat('B'));

    protected override bool TryParse(ReadOnlySpan<byte> data, out Guid value, out int bytesConsumed)
        =>Utf8Parser.TryParse(data, out value, out bytesConsumed, 'B');
}