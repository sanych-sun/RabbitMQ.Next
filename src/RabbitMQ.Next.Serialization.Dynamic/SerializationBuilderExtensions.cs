using System;

namespace RabbitMQ.Next.Serialization.Dynamic;

public static class SerializationBuilderExtensions
{
    public static TBuilder UseDynamicSerializer<TBuilder>(this ISerializationBuilder<TBuilder> builder, Action<IDynamicSerializerBuilder> dynamicSerializerBuilder)
    {
        var dynamicSerializerInnerBuilder = new DynamicSerializerBuilder();
        dynamicSerializerBuilder?.Invoke(dynamicSerializerInnerBuilder);
        
        return builder.UseSerializer(new DynamicSerializer(dynamicSerializerInnerBuilder.Serializers.ToArray()));
    }
}