using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class Int64ConverterTests
{
    [Theory]
    [MemberData(nameof(GenericTestCases))]
    public void CanFormat(long content, byte[] expected)
    {
        var converter = new Int64Converter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

        converter.Format(content, bufferWriter);

        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(ParseChunkedTestCases))]
    public void CanParse(long expected, params byte[][] parts)
    {
        var converter = new Int64Converter();
        var sequence = Helpers.MakeSequence(parts);

        var parsed = converter.Parse(sequence);

        Assert.Equal(expected, parsed);
    }
    
    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x68 } )]
    [InlineData(new byte[] { 0x34, 0x32, 0x68 } )]
    public void ParseThrowsOnWrongContent(byte[] content)
    {
        var converter = new Int64Converter();
        var sequence = new ReadOnlySequence<byte>(content);

        Assert.Throws<FormatException>(() => converter.Parse(sequence));
    }

    [Fact]
    public void ThrowsOnTooSmallBuffer()
    {
        var converter = new Int64Converter();
        var bufferWriter = new ArrayBufferWriter<byte>(1);

        Assert.Throws<OutOfMemoryException>(() => converter.Format(42L, bufferWriter));
    }

    public static IEnumerable<object[]> GenericTestCases()
    {
        yield return new object[] { 0L, new byte[] { 0x30 } };
        yield return new object[] { 1L, new byte[] { 0x31 } };
        yield return new object[] { 42L, new byte[] { 0x34, 0x32 } };
        yield return new object[] { -42L, new byte[] { 0x2D, 0x34, 0x32 } };
    }

    public static IEnumerable<object[]> ParseChunkedTestCases()
    {
        yield return new object[] { -42L, new byte[] { 0x2D, 0x34 }, new byte[] { 0x32 } };
    }
}