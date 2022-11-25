using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization.PlainText;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText;

public class PlainTextSerializerTests
{
    [Theory]
    [MemberData(nameof(EmptyFormattersTestCases))]
    public void ThrowsOnEmptyFormatters(IEnumerable<IConverter> formatters)
    {
        Assert.Throws<ArgumentNullException>(() => new PlainTextSerializer(formatters));
    }

    [Fact]
    public void SerializeThrowsOnNotSupportedTypes()
    {
        var message = Substitute.For<IMessageProperties>();
        var serializer = new PlainTextSerializer(new [] { this.MockFormatter<string>()});
        Assert.Throws<InvalidOperationException>(() => serializer.Serialize(message, 42, new ArrayBufferWriter<byte>()));
    }

    [Fact]
    public void DeserializeThrowsOnNotSupportedTypes()
    {
        var message = Substitute.For<IMessageProperties>();
        var serializer = new PlainTextSerializer(new [] { this.MockFormatter<string>()});
        Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<int>(message, ReadOnlySequence<byte>.Empty));
    }

    [Fact]
    public void SerializeTryFormattersUntilFound()
    {
        var message = Substitute.For<IMessageProperties>();
        var intFormatter = this.MockFormatter<int>();
        var stringFormatter = this.MockFormatter<string>();
        var doubleFormatter = this.MockFormatter<double>();
        var buffer = Substitute.For<IBufferWriter<byte>>();
        var data = "test string";

        var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter, doubleFormatter });
        serializer.Serialize(message, data, buffer);

        intFormatter.Received().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
        stringFormatter.Received().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
        doubleFormatter.DidNotReceive().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
    }

    [Fact]
    public void DeserializeTryFormattersUntilFound()
    {
        var message = Substitute.For<IMessageProperties>();
        var intFormatter = this.MockFormatter<int>();
        var stringFormatter = this.MockFormatter<string>();
        var doubleFormatter = this.MockFormatter<double>();

        var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter, doubleFormatter });
        serializer.Deserialize<string>(message, ReadOnlySequence<byte>.Empty);

        intFormatter.Received().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
        stringFormatter.Received().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
        doubleFormatter.DidNotReceive().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
    }

    private IConverter MockFormatter<TContent>()
    {
        var formatter = Substitute.For<IConverter>();
        formatter.TryFormat(Arg.Any<TContent>(), Arg.Any<IBufferWriter<byte>>()).Returns(true);
        formatter.TryParse(Arg.Any<ReadOnlySequence<byte>>(), out Arg.Any<TContent>()).Returns(true);
        return formatter;
    }

    public static IEnumerable<object[]> EmptyFormattersTestCases()
    {
        yield return new object[] { null };
        yield return new object[] { Enumerable.Empty<IConverter>() };
        yield return new object[] { new IConverter[0] };
    }
}