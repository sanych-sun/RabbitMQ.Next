using System;

namespace RabbitMQ.Next.Serialization.SystemJson;

public static class SerializationBuilderExtensions
{
    public static ISerializationBuilder UseSystemJsonSerializer(this ISerializationBuilder builder, Action<ISystemJsonSerializerBuilder> registration = null)
    {
        var innerBuilder = new SystemJsonSerializerBuilder();
        registration?.Invoke(innerBuilder);

        var serializer = new SystemJsonSerializer(innerBuilder.Options);
        for (var i = 0; i < innerBuilder.ContentTypes.Count; i++)
        {
            builder.UseSerializer(serializer, innerBuilder.ContentTypes[i]);
        }

        if (innerBuilder.IsDefault)
        {
            builder.DefaultSerializer(serializer);
        }

        return builder;
    }
}