using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;

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
                var s = serializers.First();
                return new StaticSerializerFactory(s.Serializer, s.Default ? null : s.ContentType);
            }

            return new DynamicSerializerFactory(serializers);
        }
    }
}