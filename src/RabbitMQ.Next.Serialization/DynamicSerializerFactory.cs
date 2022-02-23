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
                var s = serializers[i];

                if (!string.IsNullOrEmpty(s.ContentType))
                {
                    this.serializers[s.ContentType] = s.Serializer;
                }

                if (s.Default)
                {
                    this.defaultSerializer = s.Serializer;
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