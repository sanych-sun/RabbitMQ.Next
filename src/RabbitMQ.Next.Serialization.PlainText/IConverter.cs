using System.Buffers;

namespace RabbitMQ.Next.Serialization.PlainText;

public interface IConverter
{
    bool TryFormat<TContent>(TContent content, IBufferWriter<byte> writer);

    bool TryParse<TContent>(ReadOnlySequence<byte> bytes, out TContent parsed);
}