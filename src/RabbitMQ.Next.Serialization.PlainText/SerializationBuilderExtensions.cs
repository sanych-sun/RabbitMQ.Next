using System;

namespace RabbitMQ.Next.Serialization.PlainText
{
    public static class SerializationBuilderExtensions
    {
        public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, Action<IPlainTextSerializerBuilder> registration = null)
            where TBuilder : ISerializationBuilder<TBuilder>
        {
            var innerBuilder = new PlainTextSerializerBuilder();
            registration?.Invoke(innerBuilder);

            var serializer = new PlainTextSerializer(innerBuilder.Converters);

            for (var i = 0; i < innerBuilder.ContentTypes.Count; i++)
            {
                builder.UseSerializer(serializer, innerBuilder.ContentTypes[i], innerBuilder.IsDefault);
            }

            return builder;
        }
    }
}