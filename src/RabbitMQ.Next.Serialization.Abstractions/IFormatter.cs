using System.Buffers;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface IFormatter<TContent>
    {
        void Format(TContent content, IBufferWriter writer);

        TContent Parse(ReadOnlySequence<byte> bytes);
    }
}