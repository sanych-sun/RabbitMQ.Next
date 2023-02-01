using System.Text;
using Newtonsoft.Json;

namespace RabbitMQ.Next.Serialization.NewtonsoftJson;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseNewtonsoftJsonSerializer<TBuilder>(this ISerializationBuilder<TBuilder> builder, JsonSerializerSettings options = null)
        => builder.UseNewtonsoftJsonSerializer(Encoding.UTF8, options);
    
    public static TBuilder UseNewtonsoftJsonSerializer<TBuilder>(this ISerializationBuilder<TBuilder> builder, Encoding encoding, JsonSerializerSettings options = null)
    {
        options ??= new JsonSerializerSettings();
        return builder.UseSerializer(new NewtonsoftJsonSerializer(options, encoding));
    }
}