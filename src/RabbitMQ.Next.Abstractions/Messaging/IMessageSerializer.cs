using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Messaging
{
    public interface IMessageSerializer<TMessage>
    {
        void Serialize(IBufferWriter writer, TMessage message);

        TMessage Deserialize(ReadOnlySequence<byte> bytes);
    }
}