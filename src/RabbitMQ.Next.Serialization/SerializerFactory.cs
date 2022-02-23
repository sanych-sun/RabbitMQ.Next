using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMQ.Next.Serialization
{
    public static class SerializerFactory
    {
        public static ISerializerFactory Create(IReadOnlyList<(ISerializer Serializer, string ContentType, bool Default)> serializers)
        {
            if (serializers == null || serializers.Count == 0)
            {
                throw new ArgumentNullException(nameof(serializers));
            }

            if (serializers.Count == 1)
            {
                var (serializer, contentType, @default) = serializers.First();
                return new StaticSerializerFactory(serializer, @default ? null : contentType);
            }

            return new DynamicSerializerFactory(serializers);
        }
    }
}