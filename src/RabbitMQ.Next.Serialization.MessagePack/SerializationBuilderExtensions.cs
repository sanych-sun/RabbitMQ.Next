using System;

namespace RabbitMQ.Next.Serialization.MessagePack
{
    public static class SerializationBuilderExtensions
    {
        public static ISerializationBuilder UseMessagePackSerializer(this ISerializationBuilder builder, Action<IMessagePackSerializerBuilder> registration = null)
        {
            var innerBuilder = new MessagePackSerializerBuilder();
            registration?.Invoke(innerBuilder);

            var serializer = new MessagePackSerializer();
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
}