using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class TimeSpanConverterTests
{
    [Theory]
    [MemberData(nameof(GenericTestCases))]
    public void CanFormat(string dateString, byte[] expected)
    {
        var content = TimeSpan.Parse(dateString);
        var converter = new TimeSpanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

        converter.Format(content, bufferWriter);

        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(ParseChunkedTestCases))]
    public void CanParse(string dateString, params byte[][] parts)
    {
        var date = TimeSpan.Parse(dateString);
        var converter = new TimeSpanConverter();
        var sequence = Helpers.MakeSequence(parts);

        var parsed = converter.Parse(sequence);

        Assert.Equal(date, parsed);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x68 } )]
    [InlineData(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30, 0x2B, 0x2D })]
    public void ParseThrowsOnWrongContent(byte[] content)
    {
        var converter = new TimeSpanConverter();
        var sequence = new ReadOnlySequence<byte>(content);

        Assert.Throws<FormatException>(() => converter.Parse(sequence));
    }

    [Fact]
    public void ThrowsOnTooSmallBuffer()
    {
        var converter = new TimeSpanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(1);

        Assert.Throws<OutOfMemoryException>(() => converter.Format(TimeSpan.Zero, bufferWriter));
    }

    public static IEnumerable<object[]> GenericTestCases()
    {
        yield return new object[] { "00:01:01", new byte[] { 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
        yield return new object[] { "-00:01:01", new byte[] { 0x2D, 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
        yield return new object[] { "5.00:01:01", new byte[] { 0x35, 0x2E, 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
    }

    public static IEnumerable<object[]> ParseChunkedTestCases()
    {
        yield return new object[] { "5.00:01:01", new byte[] { 0x35, 0x2E, 0x30, 0x30 }, new byte[] {0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
    }
}