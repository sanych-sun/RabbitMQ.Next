using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class TimeSpanFormatterTests
{
    [Theory]
    [MemberData(nameof(GenericTestCases))]
    public void CanFormat(string dateString, byte[] expected)
    {
        var content = TimeSpan.Parse(dateString);
        var formatter = new TimeSpanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

        var result = formatter.TryFormat(content, bufferWriter);

        Assert.True(result);
        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(NullableTestCases))]
    public void CanFormatNullable(string dateString, byte[] expected)
    {
        TimeSpan? content = dateString == null ? null : TimeSpan.Parse(dateString);
        var formatter = new TimeSpanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

        var result = formatter.TryFormat(content, bufferWriter);

        Assert.True(result);
        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(ParseExtraTestCases))]
    public void CanParse(string dateString, params byte[][] parts)
    {
        var date = TimeSpan.Parse(dateString);
        var formatter = new TimeSpanConverter();
        var sequence = Helpers.MakeSequence(parts);

        var result = formatter.TryParse(sequence, out TimeSpan parsed);

        Assert.True(result);
        Assert.Equal(date, parsed);
    }

    [Theory]
    [MemberData(nameof(GenericTestCases))]
    [MemberData(nameof(NullableTestCases))]
    [MemberData(nameof(ParseExtraTestCases))]
    public void CanParseNullable(string dateString, params byte[][] parts)
    {
        TimeSpan? date = dateString == null ? null : TimeSpan.Parse(dateString);
        var formatter = new TimeSpanConverter();
        var sequence = Helpers.MakeSequence(parts);

        var result = formatter.TryParse(sequence, out TimeSpan? parsed);

        Assert.True(result);
        Assert.Equal(date, parsed);
    }


    [Theory]
    [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
    [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
    [InlineData(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30, 0x2B, 0x2D })]
    public void ParseThrowsOnWrongContent(byte[] content)
    {
        var formatter = new TimeSpanConverter();
        var sequence = new ReadOnlySequence<byte>(content);

        Assert.Throws<FormatException>(() => formatter.TryParse(sequence, out TimeSpan _));
    }

    [Fact]
    public void ThrowsOnTooSmallBuffer()
    {
        var formatter = new TimeSpanConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(1);

        Assert.Throws<OutOfMemoryException>(() => formatter.TryFormat(TimeSpan.Zero, bufferWriter));
    }

    [Fact]
    public void ShouldNotFormatWrongType()
    {
        var formatter = new TimeSpanConverter();
        var bufferWriter = Substitute.For<IBufferWriter<byte>>();

        var result = formatter.TryFormat("hello", bufferWriter);

        Assert.False(result);
        bufferWriter.DidNotReceive().Advance(Arg.Any<int>());
    }

    [Fact]
    public void ShouldNotParseToWrongType()
    {
        var formatter = new TimeSpanConverter();
        var sequence = Helpers.MakeSequence(new byte[] { 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 });

        var result = formatter.TryParse<string>(sequence, out var _);

        Assert.False(result);
    }

    public static IEnumerable<object[]> GenericTestCases()
    {
        yield return new object[] { "00:01:01", new byte[] { 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
        yield return new object[] { "-00:01:01", new byte[] { 0x2D, 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
        yield return new object[] { "5.00:01:01", new byte[] { 0x35, 0x2E, 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
    }

    public static IEnumerable<object[]> NullableTestCases()
    {
        yield return new object[] { null, Array.Empty<byte>() };
    }

    public static IEnumerable<object[]> ParseExtraTestCases()
    {
        yield return new object[] { "5.00:01:01", new byte[] { 0x35, 0x2E, 0x30, 0x30 }, new byte[] {0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 } };
    }
}