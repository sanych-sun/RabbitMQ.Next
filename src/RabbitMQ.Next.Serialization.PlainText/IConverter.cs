using System.Buffers;

namespace RabbitMQ.Next.Serialization.PlainText;

public interface IConverter<TContent>
{
    void Format(TContent content, IBufferWriter<byte> writer);

    TContent Parse(ReadOnlySequence<byte> bytes);
}