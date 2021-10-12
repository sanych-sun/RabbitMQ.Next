using System;
using System.Buffers;
using System.Collections.Generic;
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

            formatter.Format(date, bufferWriter);

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

            formatter.Format(date, bufferWriter);

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

            var result = formatter.Parse<DateTimeOffset>(sequence);

            Assert.Equal(date, result);
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

            var result = formatter.Parse<DateTimeOffset?>(sequence);

            Assert.Equal(date, result);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30, 0x2B, 0x2D })]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new DateTimeOffsetFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<DateTimeOffset>(sequence));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.Format(DateTimeOffset.UtcNow, bufferWriter));
        }

        [Theory]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(DateTimeOffset), true)]
        [InlineData(typeof(DateTimeOffset?), true)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new DateTimeOffsetFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new DateTimeOffsetFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new DateTimeOffsetFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30 });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
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