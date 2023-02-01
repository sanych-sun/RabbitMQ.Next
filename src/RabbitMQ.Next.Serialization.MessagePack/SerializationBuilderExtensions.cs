using MessagePack;

namespace RabbitMQ.Next.Serialization.MessagePack;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseMessagePackSerializer<TBuilder>(this ISerializationBuilder<TBuilder> builder, MessagePackSerializerOptions options = null)
    {
        options ??= global::MessagePack.Resolvers.ContractlessStandardResolver.Options;

        return builder.UseSerializer(new MessagePackSerializer(options));
    }
}