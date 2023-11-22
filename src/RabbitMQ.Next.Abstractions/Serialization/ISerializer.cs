using System.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization;

public interface ISerializer
{
    void Serialize<TContent>(IMessageProperties properties, TContent content, IBufferWriter<byte> writer);

    TContent Deserialize<TContent>(IMessageProperties properties, ReadOnlySequence<byte> bytes);
}