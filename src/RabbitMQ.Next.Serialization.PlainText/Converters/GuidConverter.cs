using System;
using System.Buffers;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public class GuidConverter: SimpleConverterBase<Guid>
    {
        protected override bool TryFormatContent(Guid content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten, new StandardFormat('B'));

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out Guid value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed, 'B');
    }
}