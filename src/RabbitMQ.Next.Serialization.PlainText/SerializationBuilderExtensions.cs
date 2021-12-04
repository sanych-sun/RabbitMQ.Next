using System;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.PlainText
{
    public static class SerializationBuilderExtensions
    {
        public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, Action<IPlainTextSerializerBuilder> registration = null)
            where TBuilder: ISerializationBuilder<TBuilder>
        {
            var innerBuilder = new PlainTextSerializerBuilder();
            registration?.Invoke(innerBuilder);

            var serializer = new PlainTextSerializer(innerBuilder.Formatters);

            if (innerBuilder.ContentTypes.Count > 1)
            {
                for (var i = 0; i < innerBuilder.ContentTypes.Count; i++)
                {
                    builder.UseSerializer(serializer, innerBuilder.ContentTypes[i], innerBuilder.IsDefault);
                }
            }
            else
            {
                builder.UseSerializer(serializer, innerBuilder.ContentTypes?.First(), innerBuilder.IsDefault);
            }

            return builder;
        }
    }
}