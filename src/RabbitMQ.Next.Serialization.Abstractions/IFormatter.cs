using System.Buffers;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface IFormatter<TContent>
    {
        void Format(TContent content, IBufferWriter<byte> writer);

        TContent Parse(ReadOnlySequence<byte> bytes);
    }
}