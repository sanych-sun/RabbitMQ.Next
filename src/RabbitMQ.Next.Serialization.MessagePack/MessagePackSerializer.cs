using System.Buffers;
using MessagePack;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.MessagePack;

internal class MessagePackSerializer : ISerializer
{
    private readonly MessagePackSerializerOptions options;

    public MessagePackSerializer(MessagePackSerializerOptions options)
    {
        this.options = options;
    }

    public void Serialize<TContent>(IMessageProperties message, TContent content, IBufferWriter<byte> writer)
    {
        global::MessagePack.MessagePackSerializer.Serialize(writer, content, this.options);
    }

    public TContent Deserialize<TContent>(IMessageProperties message, ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return default;
        }

        return global::MessagePack.MessagePackSerializer.Deserialize<TContent>(bytes, this.options);
    }
}