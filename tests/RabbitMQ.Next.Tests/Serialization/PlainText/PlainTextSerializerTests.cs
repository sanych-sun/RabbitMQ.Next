using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization.PlainText;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText;

public class PlainTextSerializerTests
{
    [Theory]
    [MemberData(nameof(EmptyConvertersTestCases))]
    public void ThrowsOnEmptyConverters(IReadOnlyDictionary<Type, object> converters)
    {
        Assert.Throws<ArgumentNullException>(() => new PlainTextSerializer(converters));
    }
    
    [Fact]
    public void ThrowsOnMissConfiguredConverters()
    {
        var converters = new Dictionary<Type, object>
        {
            [typeof(string)] = Substitute.For<IConverter<int>>(),
        };
        
        Assert.Throws<ArgumentException>(() => new PlainTextSerializer(converters));
    }

    [Fact]
    public void SerializeThrowsOnNotSupportedTypes()
    {
        var message = Substitute.For<IMessageProperties>();
        var converters = new Dictionary<Type, object>
        {
            [typeof(string)] = Substitute.For<IConverter<string>>(),
        };
        var serializer = new PlainTextSerializer(converters);
        
        Assert.Throws<NotSupportedException>(() => serializer.Serialize(message, 42, new ArrayBufferWriter<byte>()));
    }

    [Fact]
    public void DeserializeThrowsOnNotSupportedTypes()
    {
        var message = Substitute.For<IMessageProperties>();
        var converters = new Dictionary<Type, object>
        {
            [typeof(string)] = Substitute.For<IConverter<string>>(),
        };
        var serializer = new PlainTextSerializer(converters);
        
        Assert.Throws<NotSupportedException>(() => serializer.Deserialize<int>(message, ReadOnlySequence<byte>.Empty));
    }

    public static IEnumerable<object[]> EmptyConvertersTestCases()
    {
        yield return new object[] { null };
        yield return new object[] { new Dictionary<Type, object>() };
    }
}
