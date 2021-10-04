using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IReadOnlyDictionary<string, ISerializer> serializers;
        private readonly ISerializer defaultSerializer;

        public SerializerFactory(IReadOnlyDictionary<string, ISerializer> serializers)
        {
            this.serializers = serializers;
            if (serializers.TryGetValue(string.Empty, out var serializer))
            {
                this.defaultSerializer = serializer;
            }
        }

        public ISerializer Get(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return this.defaultSerializer;
            }

            if (this.serializers.TryGetValue(contentType, out var serializer))
            {
                return serializer;
            }

            return null;
        }
    }
}