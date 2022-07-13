using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

public class TimeSpanConverter: SimpleConverterBase<TimeSpan>
{
    protected override bool TryFormatContent(TimeSpan content, Span<byte> target, out int bytesWritten)
        => Utf8Formatter.TryFormat(content, target, out bytesWritten);

    protected override bool TryParseContent(ReadOnlySpan<byte> data, out TimeSpan value, out int bytesConsumed)
        =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
}