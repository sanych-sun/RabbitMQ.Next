using System;
using System.Buffers;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Formatters
{
    internal class DateTimeOffsetFormatter : SimpleFormatterBase<DateTimeOffset>
    {
        protected override bool TryFormatContent(DateTimeOffset content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten, new StandardFormat('O'));

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out DateTimeOffset value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed, 'O');
    }
}