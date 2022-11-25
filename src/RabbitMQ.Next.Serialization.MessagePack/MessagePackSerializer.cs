using System.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.MessagePack;

internal class MessagePackSerializer : ISerializer
{
    public void Serialize<TContent>(IMessageProperties message, TContent content, IBufferWriter<byte> writer)
    {
        global::MessagePack.MessagePackSerializer.Serialize(writer, content);
    }

    public TContent Deserialize<TContent>(IMessageProperties message, ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return default;
        }

        return global::MessagePack.MessagePackSerializer.Deserialize<TContent>(bytes);
    }
}