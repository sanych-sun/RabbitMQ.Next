using System;
using System.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.Dynamic;

internal sealed class DynamicSerializer: ISerializer
{
    private readonly (Func<IMessageProperties, bool> predicate, ISerializer serializer)[] serializers;

    public DynamicSerializer((Func<IMessageProperties, bool> predicate, ISerializer serializer)[] serializers)
    {
        this.serializers = serializers;
    }

    public void Serialize<TContent>(IMessageProperties properties, TContent content, IBufferWriter<byte> writer)
        => this.GetSerializer(properties).Serialize(properties, content, writer);

    public TContent Deserialize<TContent>(IMessageProperties properties, ReadOnlySequence<byte> bytes)
        => this.GetSerializer(properties).Deserialize<TContent>(properties, bytes);

    private ISerializer GetSerializer(IMessageProperties properties)
    {
        for (int i = 0; i < this.serializers.Length; i++)
        {
            if (this.serializers[i].predicate(properties))
            {
                return this.serializers[i].serializer;
            }
        }

        throw new NotSupportedException();
    }
}