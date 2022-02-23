using System;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.SystemJson
{
    public static class SerializationBuilderExtensions
    {
        public static TBuilder UseSystemJsonSerializer<TBuilder>(this TBuilder builder, Action<ISystemJsonSerializerBuilder> registration = null)
            where TBuilder : ISerializationBuilder<TBuilder>
        {
            var innerBuilder = new SystemJsonSerializerBuilder();
            registration?.Invoke(innerBuilder);

            var serializer = new SystemJsonSerializer(innerBuilder.Options);
            for (var i = 0; i < innerBuilder.ContentTypes.Count; i++)
            {
                builder.UseSerializer(serializer, innerBuilder.ContentTypes[i], innerBuilder.IsDefault);
            }

            return builder;
        }
    }
}