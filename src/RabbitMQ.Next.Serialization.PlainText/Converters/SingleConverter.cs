using System;
using System.Buffers.Text;

namespace RabbitMQ.Next.Serialization.PlainText.Converters
{
    public class SingleConverter : SimpleConverterBase<float>
    {
        protected override bool TryFormatContent(float content, Span<byte> target, out int bytesWritten)
            => Utf8Formatter.TryFormat(content, target, out bytesWritten);

        protected override bool TryParseContent(ReadOnlySpan<byte> data, out float value, out int bytesConsumed)
            =>Utf8Parser.TryParse(data, out value, out bytesConsumed);
    }
}