using System.Buffers;

namespace RabbitMQ.Next.Serialization.PlainText
{
    public interface IFormatter
    {
        bool TryFormat<TContent>(TContent content, IBufferWriter<byte> writer);

        bool TryParse<TContent>(ReadOnlySequence<byte> bytes, out TContent parsed);
    }
}