using System;
using System.Linq;

namespace RabbitMQ.Next.Serialization.MessagePack;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseMessagePackSerializer<TBuilder>(this TBuilder builder, Action<IMessagePackSerializerBuilder> registration = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        var innerBuilder = new MessagePackSerializerBuilder();
        registration?.Invoke(innerBuilder);

        var serializer = new MessagePackSerializer();
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