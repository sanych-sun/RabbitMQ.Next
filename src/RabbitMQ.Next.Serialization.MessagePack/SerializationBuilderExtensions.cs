using System;

namespace RabbitMQ.Next.Serialization.MessagePack
{
    public static class SerializationBuilderExtensions
    {
        public static TBuilder UseMessagePackSerializer<TBuilder>(this TBuilder builder, Action<IMessagePackSerializerBuilder> registration = null)
            where TBuilder : ISerializationBuilder<TBuilder>
        {
            var innerBuilder = new MessagePackSerializerBuilder();
            registration?.Invoke(innerBuilder);

            var serializer = new MessagePackSerializer();
            for (var i = 0; i < innerBuilder.ContentTypes.Count; i++)
            {
                builder.UseSerializer(serializer, innerBuilder.ContentTypes[i], innerBuilder.IsDefault);
            }

            return builder;
        }
    }
}