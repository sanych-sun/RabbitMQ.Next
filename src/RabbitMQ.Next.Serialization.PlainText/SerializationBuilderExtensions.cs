using System;

namespace RabbitMQ.Next.Serialization.PlainText;

public static class SerializationBuilderExtensions
{

    public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, Action<IPlainTextSerializerBuilder> serializerBuilder = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        serializerBuilder ??= b => b.UseDefaultConverters();
        
        var plainTextSerializerBuilder = new PlainTextSerializerBuilder();
        serializerBuilder.Invoke(plainTextSerializerBuilder);
        
        builder.UseSerializer(new PlainTextSerializer(plainTextSerializerBuilder.Converters));

        return builder;
    }
}
