using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public class SByteConverter: SimpleConverterBase<sbyte>
    {
        protected override bool TryFormatContent(sbyte content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten);

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out sbyte value, out int bytesConsumed)
            => Utf8Parser.TryParse(data, out value, out bytesConsumed);
    }
}