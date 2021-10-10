using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly Dictionary<string, ISerializer> serializers = new();
        private ISerializer defaultSerializer;

        public void RegisterSerializer(ISerializer serializer, IReadOnlyList<string> contentTypes = null, bool isDefault = true)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (isDefault)
            {
                this.defaultSerializer = serializer;
            }

            if (contentTypes != null)
            {
                for (var i = 0; i < contentTypes.Count; i++)
                {
                    this.serializers[contentTypes[i]] = serializer;
                }
            }
        }

        ISerializer ISerializerFactory.Get(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return this.defaultSerializer;
            }

            if (this.serializers.TryGetValue(contentType, out var serializer))
            {
                return serializer;
            }

            return this.defaultSerializer;
        }
    }
}