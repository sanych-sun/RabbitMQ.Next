using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Serialization;

internal class SerializerFactory : ISerializerFactory, ISerializationBuilder
{
    private readonly Dictionary<string, ISerializer> serializers = new();
    private ISerializer defaultSerializer;

    public ISerializer Get(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return this.defaultSerializer;
        }
            
        if (this.serializers.TryGetValue(contentType.ToLowerInvariant(), out var serializer))
        {
            return serializer;
        }

        return this.defaultSerializer ?? throw new NotSupportedException($"Cannot resolve serializer for '{contentType}' content type.");
    }

    public ISerializationBuilder DefaultSerializer(ISerializer serializer)
    {
        this.defaultSerializer = serializer;
        return this;
    }

    public ISerializationBuilder UseSerializer(ISerializer serializer, string contentType)
    {
        this.serializers[contentType.ToLowerInvariant()] = serializer;
        return this;
    }
}