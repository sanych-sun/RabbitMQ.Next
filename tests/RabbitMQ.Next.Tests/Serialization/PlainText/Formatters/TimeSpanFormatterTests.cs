using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class TimeSpanFormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(string dateString, byte[] expected)
        {
            var date = TimeSpan.Parse(dateString);
            var formatter = new TimeSpanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            formatter.Format(date, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(string dateString, byte[] expected)
        {
            TimeSpan? date = dateString == null ? null : TimeSpan.Parse(dateString);
            var formatter = new TimeSpanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            formatter.Format(date, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(string dateString, params byte[][] parts)
        {
            var date = TimeSpan.Parse(dateString);
            var formatter = new TimeSpanFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<TimeSpan>(sequence);

            Assert.Equal(date, result);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(string dateString, params byte[][] parts)
        {
            TimeSpan? date = dateString == null ? null : TimeSpan.Parse(dateString);
            var formatter = new TimeSpanFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<TimeSpan?>(sequence);

            Assert.Equal(date, result);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x32, 0x30, 0x30, 0x39, 0x2D, 0x30, 0x36, 0x2D, 0x31, 0x35, 0x54, 0x31, 0x33, 0x3A, 0x34, 0x35, 0x3A, 0x33, 0x30, 0x2E, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x37, 0x3A, 0x30, 0x30, 0x2B, 0x2D })]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new TimeSpanFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<TimeSpan>(sequence));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new TimeSpanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.Format(TimeSpan.Zero, bufferWriter));
        }

        [Theory]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(TimeSpan), true)]
        [InlineData(typeof(TimeSpan?), true)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new TimeSpanFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new TimeSpanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new TimeSpanFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x30, 0x30, 0x3A, 0x30, 0x31, 0x3A, 0x30, 0x31 });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
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
}