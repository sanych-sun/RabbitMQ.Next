using System.Text.Json;

namespace RabbitMQ.Next.Serialization.SystemJson;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseSystemJsonSerializer<TBuilder>(this ISerializationBuilder<TBuilder> builder, JsonSerializerOptions options = null)
    {
        options ??= new JsonSerializerOptions(JsonSerializerDefaults.General);
        return builder.UseSerializer(new SystemJsonSerializer(options));
    }
}