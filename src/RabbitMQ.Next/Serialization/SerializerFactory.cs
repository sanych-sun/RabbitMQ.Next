using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization;

public class SerializerFactory : ISerializerFactory
{
    private readonly List<(ISerializer serializer, Func<IMessageProperties, bool> predicate)> serializers = new();
    private ISerializer defaultSerializer;

    public ISerializer Get(IMessageProperties message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        
        for(var i = 0; i < this.serializers.Count; i++)
        {
            if (this.serializers[i].predicate(message))
            {
                return this.serializers[i].serializer;    
            }
        }

        return this.defaultSerializer ?? throw new NotSupportedException($"Cannot resolve serializer for the message.");
    }

    public void DefaultSerializer(ISerializer serializer)
    {
        this.defaultSerializer = serializer;
    }

    public void UseSerializer(ISerializer serializer, Func<IMessageProperties, bool> predicate)
    {
        this.serializers.Add((serializer, predicate));
    }
}