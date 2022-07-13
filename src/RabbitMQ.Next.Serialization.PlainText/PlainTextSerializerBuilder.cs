using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;

namespace RabbitMQ.Next.Serialization.PlainText;

internal class PlainTextSerializerBuilder : IPlainTextSerializerBuilder
{
    private static readonly string[] DefaultContentTypes = { "text/plain" };
    private static readonly IConverter[] DefaultFormatters = {
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

    private List<string> contentTypes;
    private List<IConverter> converters;
    public bool IsDefault { get; private set; } = true;

    public IReadOnlyList<string> ContentTypes
        => this.contentTypes == null ? DefaultContentTypes : this.contentTypes;

    public IReadOnlyList<IConverter> Converters
        => this.converters == null ? DefaultFormatters : this.converters;

    IPlainTextSerializerBuilder IPlainTextSerializerBuilder.AsDefault()
    {
        this.IsDefault = true;
        return this;
    }

    IPlainTextSerializerBuilder IPlainTextSerializerBuilder.ContentType(string contentType)
    {
        this.contentTypes ??= new List<string>(DefaultContentTypes);
        this.contentTypes.Add(contentType);

        return this;
    }

    IPlainTextSerializerBuilder IPlainTextSerializerBuilder.UseConverter(IConverter converter)
    {
        this.converters ??= new List<IConverter>(DefaultFormatters);
        this.converters.Add(converter);

        return this;
    }
}