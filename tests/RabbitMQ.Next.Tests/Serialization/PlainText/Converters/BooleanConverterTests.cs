using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class BooleanConverterTests
{
    [Theory]
    [MemberData(nameof(GenericTestCases))]
    public void CanFormat(bool content, byte[] expected)
    {
        var converter = new BooleanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

        converter.Format(content, bufferWriter);

        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(ParseChunkedTestCases))]
    public void CanParse(bool expected, params byte[][] parts)
    {
        var converter = new BooleanConverter();
        var sequence = Helpers.MakeSequence(parts);

        var parsed = converter.Parse(sequence);

        Assert.Equal(expected, parsed);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x54, 0x72, 0x75 } )]
    [InlineData(new byte[] { 0x54, 0x72, 0x75, 0x65, 0x6F } )]
    public void ParseThrowsOnWrongContent(byte[] content)
    {
        var converter = new BooleanConverter();
        var sequence = new ReadOnlySequence<byte>(content);

        Assert.Throws<FormatException>(() =>  converter.Parse(sequence));
    }

    [Fact]
    public void ThrowsOnTooSmallBuffer()
    {
        var converter = new BooleanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(1);

        Assert.Throws<OutOfMemoryException>(() => converter.Format(true, bufferWriter));
    }
    

    public static IEnumerable<object[]> GenericTestCases()
    {
        yield return new object[] { true, new byte[] { 0x54, 0x72, 0x75, 0x65 } };
        yield return new object[] { false, new byte[] { 0x46, 0x61, 0x6C, 0x73, 0x65 } };
    }

    public static IEnumerable<object[]> ParseChunkedTestCases()
    {
        yield return new object[] { true, new byte[] { 0x54, 0x72 }, new byte[] { 0x75, 0x65 } };
    }
}