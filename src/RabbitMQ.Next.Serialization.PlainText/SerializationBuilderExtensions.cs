using System;
using System.Linq;

namespace RabbitMQ.Next.Serialization.PlainText;

public static class SerializationBuilderExtensions
{
    public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, Action<IPlainTextSerializerBuilder> registration = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        var innerBuilder = new PlainTextSerializerBuilder();
        registration?.Invoke(innerBuilder);

        var serializer = new PlainTextSerializer(innerBuilder.Converters);
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