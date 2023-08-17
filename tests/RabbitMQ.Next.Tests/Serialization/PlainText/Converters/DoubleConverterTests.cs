using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class DoubleConverterTests
{
    [Theory]
    [MemberData(nameof(GenericTestCases))]
    public void CanFormat(double content, byte[] expected)
    {
        var converter = new DoubleConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

        converter.Format(content, bufferWriter);

        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(ParseChunkedTestCases))]
    public void CanParse(double expected, params byte[][] parts)
    {
        var converter = new DoubleConverter();
        var sequence = Helpers.MakeSequence(parts);

        var parsed = converter.Parse(sequence);

        Assert.Equal(expected, parsed);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
    [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
    public void ParseThrowsOnWrongContent(byte[] content)
    {
        var converter = new DoubleConverter();
        var sequence = new ReadOnlySequence<byte>(content);

        Assert.Throws<FormatException>(() => converter.Parse(sequence));
    }

    [Fact]
    public void ThrowsOnTooSmallBuffer()
    {
        var converter = new DoubleConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(1);

        Assert.Throws<OutOfMemoryException>(() => converter.Format(42, bufferWriter));
    }

    public static IEnumerable<object[]> GenericTestCases()
    {
        yield return new object[] { 0, new byte[] { 0x30 } };
        yield return new object[] { 1, new byte[] { 0x31 } };
        yield return new object[] { 4.2, new byte[] { 0x34, 0x2e, 0x32 } };
        yield return new object[] { -4.2, new byte[] { 0x2D, 0x34, 0x2e, 0x32 } };
    }

    public static IEnumerable<object[]> ParseChunkedTestCases()
    {
        yield return new object[] { -4.2, new byte[] { 0x2D, 0x34 }, new byte[] { 0x2e, 0x32 } };
    }
}