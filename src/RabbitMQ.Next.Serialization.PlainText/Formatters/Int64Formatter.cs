using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    internal class Int64Formatter : SimpleFormatterBase<long>
    {
        protected override bool TryFormatContent(long content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten);

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out long value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
    }
}