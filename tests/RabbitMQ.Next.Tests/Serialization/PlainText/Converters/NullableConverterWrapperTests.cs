using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class NullableConverterWrapperTests
{
    [Fact]
    public void CanFormat()
    {
        var wrappedConverter = Substitute.For<IConverter<int>>();
        var nullableConverter = new NullableConverterWrapper<int>(wrappedConverter);
        var bufferWriter = new ArrayBufferWriter<byte>(10);
        
        nullableConverter.Format(null, bufferWriter);

        Assert.True(bufferWriter.WrittenMemory.IsEmpty);
    }
    
    [Fact]
    public void CanParse()
    {
        var wrappedConverter = Substitute.For<IConverter<int>>();
        var nullableConverter = new NullableConverterWrapper<int>(wrappedConverter);
        var data = ReadOnlySequence<byte>.Empty;
        
        var parsed = nullableConverter.Parse(data);

        Assert.False(parsed.HasValue);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void FormatCallWrappedConverter(int value)
    {
        var wrappedConverter = Substitute.For<IConverter<int>>();
        var nullableConverter = new NullableConverterWrapper<int>(wrappedConverter);
        var bufferWriter = new ArrayBufferWriter<byte>(10);
        
        nullableConverter.Format(value, bufferWriter);

        wrappedConverter.Received().Format(value, bufferWriter);
    }
    
    [Theory]
    [InlineData(new byte[] { 0x34, 0x32, 0x68  } )]
    public void ParseCallWrappedConverter(byte[] content)
    {
        var wrappedConverter = Substitute.For<IConverter<int>>();
        var nullableConverter = new NullableConverterWrapper<int>(wrappedConverter);
        var sequence = new ReadOnlySequence<byte>(content);
        
        nullableConverter.Parse(sequence);

        wrappedConverter.Received().Parse(sequence);
    }
}