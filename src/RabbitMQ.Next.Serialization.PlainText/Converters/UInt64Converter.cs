using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public class UInt64Converter : SimpleConverterBase<ulong>
    {
        protected override bool TryFormatContent(ulong content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten);

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out ulong value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
    }
}