using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public class UInt16Converter: SimpleConverterBase<ushort>
    {
        protected override bool TryFormatContent(ushort content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten);

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out ushort value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
    }
}