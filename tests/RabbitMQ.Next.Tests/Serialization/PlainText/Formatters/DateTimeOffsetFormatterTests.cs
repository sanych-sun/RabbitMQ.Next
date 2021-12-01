using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class DateTimeOffsetFormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(string dateString, byte[] expected)
        {
            var date = DateTimeOffset.Parse(dateString);
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            var result = formatter.TryFormat(date, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(string dateString, byte[] expected)
        {
            DateTimeOffset? date = dateString == null ? null : DateTimeOffset.Parse(dateString);
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            var result = formatter.TryFormat(date, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(string dateString, params byte[][] parts)
        {
            var date = DateTimeOffset.Parse(dateString);
            var formatter = new DateTimeOffsetFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out DateTimeOffset parsed);

            Assert.True(result);
            Assert.Equal(date, parsed);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(string dateString, params byte[][] parts)
        {
            DateTimeOffset? date = dateString == null ? null : DateTimeOffset.Parse(dateString);
            var formatter = new DateTimeOffsetFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out DateTimeOffset? parsed);

            Assert.True(result);
            Assert.Equal(date, parsed);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30, 0x2B, 0x2D })]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new DateTimeOffsetFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() => formatter.TryParse(sequence, out DateTimeOffset _));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.TryFormat(DateTimeOffset.UtcNow, bufferWriter));
        }

        [Fact]
        public void ShouldNotFormatWrongType()
        {
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = Substitute.For<IBufferWriter<byte>>();

            var result = formatter.TryFormat("hello", bufferWriter);

            Assert.False(result);
            bufferWriter.DidNotReceive().Advance(Arg.Any<int>());
        }

        [Fact]
        public void ShouldNotParseToWrongType()
        {
            var formatter = new DateTimeOffsetFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30 });

            var result = formatter.TryParse<string>(sequence, out var _);

            Assert.False(result);
        }

        public static IEnumerable<object[]> GenericTestCases()
        {
            yield return new object[] { "2009-06-15T13:45:30-07:00", new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30 } };
            yield return new object[] { "2009-06-15T13:45:30+00:00", new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2B, 0x30, 0x30, 0x3A, 0x30, 0x30 } };
        }

        public static IEnumerable<object[]> NullableTestCases()
        {
            yield return new object[] { null, Array.Empty<byte>() };
        }

        public static IEnumerable<object[]> ParseExtraTestCases()
        {
            yield return new object[] { "2009-06-15T13:45:30-07:00", new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30 }, new byte[] { 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30 } };
            yield return new object[] { "2009-06-15T13:45:30-07:00", new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30}, new byte[] { 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 }, new byte[] { 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30 } };
        }
    }
}