namespace RabbitMQ.Next.Serialization.MessagePack;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseMessagePackSerializer<TBuilder>(this TBuilder builder)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        builder.UseSerializer(new MessagePackSerializer());

        return builder;
    }
}