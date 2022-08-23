using System;
using System.Linq;

namespace RabbitMQ.Next.Serialization.SystemJson;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseSystemJsonSerializer<TBuilder>(this TBuilder builder, Action<ISystemJsonSerializerBuilder> registration = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        var innerBuilder = new SystemJsonSerializerBuilder();
        registration?.Invoke(innerBuilder);

        var serializer = new SystemJsonSerializer(innerBuilder.Options);
        if (innerBuilder.ContentTypes.Count > 0)
        {
            builder.UseSerializer(serializer, message => innerBuilder.ContentTypes.Contains(message.ContentType, StringComparer.OrdinalIgnoreCase));
        }

        if (innerBuilder.IsDefault)
        {
            builder.DefaultSerializer(serializer);
        }

        return builder;
    }
}