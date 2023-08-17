using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText;

public class PlainTextSerializerBuilderTests
{
    [Fact]
    public void CanRegisterConverter()
    {
        var builder = new PlainTextSerializerBuilder();
        var converter = Substitute.For<IConverter<string>>();

        builder.UseConverter(converter);
        
        Assert.True(builder.Converters.ContainsKey(typeof(string)));
        Assert.True(builder.Converters[typeof(string)] is IConverter<string>);
    }
    
    [Fact]
    public void CanRegisterValueTypeConverter()
    {
        var builder = new PlainTextSerializerBuilder();
        var converter = Substitute.For<IConverter<int>>();

        builder.UseConverter(converter);
        
        Assert.True(builder.Converters.ContainsKey(typeof(int)));
        Assert.True(builder.Converters[typeof(int)] is IConverter<int>);
        Assert.False(builder.Converters.ContainsKey(typeof(int?)));
    }
    
    [Fact]
    public void CanRegisterValueTypeConverterWithNullableWrapper()
    {
        var builder = new PlainTextSerializerBuilder();
        var converter = Substitute.For<IConverter<int>>();

        builder.UseConverter(converter, true);
        
        Assert.True(builder.Converters.ContainsKey(typeof(int)));
        Assert.True(builder.Converters[typeof(int)] is IConverter<int>);
        Assert.True(builder.Converters.ContainsKey(typeof(int?)));
        Assert.True(builder.Converters[typeof(int?)] is IConverter<int?>);
    }
}