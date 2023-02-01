using System;
using System.Text;
using Newtonsoft.Json;

namespace RabbitMQ.Next.Serialization.NewtonsoftJson;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseNewtonsoftJsonSerializer<TBuilder>(this TBuilder builder, Func<JsonSerializerSettings> configure = null)
        where TBuilder : ISerializationBuilder<TBuilder>
        => builder.UseNewtonsoftJsonSerializer(Encoding.UTF8, configure);
    
    public static TBuilder UseNewtonsoftJsonSerializer<TBuilder>(this TBuilder builder, Encoding encoding, Func<JsonSerializerSettings> configure = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        var options = configure?.Invoke() ?? new JsonSerializerSettings();
        builder.UseSerializer(new NewtonsoftJsonSerializer(options, encoding));

        return builder;
    }
}