using System;

namespace RabbitMQ.Next.Serialization.PlainText;

public static class SerializationBuilderExtensions
{
    public static ISerializationBuilder UsePlainTextSerializer(this ISerializationBuilder builder, Action<IPlainTextSerializerBuilder> registration = null)
    {
        var innerBuilder = new PlainTextSerializerBuilder();
        registration?.Invoke(innerBuilder);

        var serializer = new PlainTextSerializer(innerBuilder.Converters);

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