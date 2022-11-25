using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;

namespace RabbitMQ.Next.Serialization.PlainText;

public static class SerializationBuilderExtensions
{
    public static readonly IReadOnlyList<IConverter> DefaultConverters = new IConverter[]
    {
        new StringConverter(),
        new GuidConverter(),
        new DateTimeOffsetConverter(),
        new TimeSpanConverter(),
        new ByteConverter(),
        new Int16Converter(),
        new Int32Converter(),
        new Int64Converter(),
        new SingleConverter(),
        new DoubleConverter(),
        new DecimalConverter(),
        new BooleanConverter(),
        new SByteConverter(),
        new UInt16Converter(),
        new UInt32Converter(),
        new UInt64Converter(),
    };

    public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, IEnumerable<IConverter> converters = null)
        where TBuilder : ISerializationBuilder<TBuilder>
    {
        builder.UseSerializer(new PlainTextSerializer(converters ?? DefaultConverters));

        return builder;
    }
}