using RabbitMQ.Next.Serialization.PlainText.Converters;

namespace RabbitMQ.Next.Serialization.PlainText;

public static class PlainTextSerializerBuilderExtensions
{
    public static IPlainTextSerializerBuilder UseDefaultConverters(this IPlainTextSerializerBuilder serializerBuilder)
    {
        serializerBuilder.UseConverter(new StringConverter());
        serializerBuilder.UseConverter(new GuidConverter(), true);
        serializerBuilder.UseConverter(new DateTimeOffsetConverter(), true);
        serializerBuilder.UseConverter(new TimeSpanConverter(), true);
        serializerBuilder.UseConverter(new ByteConverter(), true);
        serializerBuilder.UseConverter(new Int16Converter(), true);
        serializerBuilder.UseConverter(new Int32Converter(), true);
        serializerBuilder.UseConverter(new Int64Converter(), true);
        serializerBuilder.UseConverter(new SingleConverter(), true);
        serializerBuilder.UseConverter(new DoubleConverter(), true);
        serializerBuilder.UseConverter(new DecimalConverter(), true);
        serializerBuilder.UseConverter(new BooleanConverter(), true);
        serializerBuilder.UseConverter(new SByteConverter(), true);
        serializerBuilder.UseConverter(new UInt16Converter(), true);
        serializerBuilder.UseConverter(new UInt32Converter(), true);
        serializerBuilder.UseConverter(new UInt64Converter(), true);

        return serializerBuilder;
    }
}