using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization
{
    internal class DynamicSerializerFactory : ISerializerFactory
    {
        private readonly Dictionary<string, ISerializer> serializers = new();
        private readonly ISerializer defaultSerializer;

        public DynamicSerializerFactory(IReadOnlyList<(ISerializer Serializer, string ContentType, bool Default)> serializers)
        {
            if (serializers == null || serializers.Count == 0)
            {
                throw new ArgumentNullException(nameof(serializers));
            }

            for(var i = 0; i < serializers.Count; i++)
            {
                var (serializer, contentType, @default) = serializers[i];

                if (!string.IsNullOrEmpty(contentType))
                {
                    this.serializers[contentType] = serializer;
                }

                if (@default)
                {
                    this.defaultSerializer = serializer;
                }
            }
        }

        public ISerializer Get(IMessageProperties message)
        {
            if (this.serializers.TryGetValue(message.ContentType, out var serializer))
            {
                return serializer;
            }

            return this.defaultSerializer ?? throw new NotSupportedException($"Cannot resolve serializer for '{message.ContentType}' content type.");
        }
    }
}