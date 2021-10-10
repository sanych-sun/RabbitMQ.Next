using System;
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

            builder.UseSerializer(serializer, innerBuilder.ContentTypes, innerBuilder.IsDefault);

            return builder;
        }
    }
}