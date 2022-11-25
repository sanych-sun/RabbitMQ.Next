using System;
using System.Text.Json;

namespace RabbitMQ.Next.Serialization.SystemJson;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseSystemJsonSerializer<TBuilder>(this TBuilder builder, Func<JsonSerializerOptions> configure = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        var options = configure?.Invoke() ?? new JsonSerializerOptions(JsonSerializerDefaults.General);
        builder.UseSerializer(new SystemJsonSerializer(options));

        return builder;
    }
}