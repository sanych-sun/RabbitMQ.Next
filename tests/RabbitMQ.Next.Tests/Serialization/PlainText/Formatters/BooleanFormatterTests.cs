using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class BooleanFormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(bool content, byte[] expected)
        {
            var formatter = new BooleanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(bool? content, byte[] expected)
        {
            var formatter = new BooleanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(bool expected, params byte[][] parts)
        {
            var formatter = new BooleanFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<bool>(sequence);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanParseNullable(bool? expected, params byte[][] parts)
        {
            var formatter = new BooleanFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<bool?>(sequence);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new BooleanFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<bool>(sequence));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new BooleanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.Format(true, bufferWriter));
        }

        [Theory]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(bool?), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(DateTimeOffset), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new BooleanFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new BooleanFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new BooleanFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x54, 0x72, 0x75, 0x65 });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
        }

        public static IEnumerable<object[]> GenericTestCases()
        {
            yield return new object[] { true, new byte[] { 0x54, 0x72, 0x75, 0x65 } };
            yield return new object[] { false, new byte[] { 0x46, 0x61, 0x6C, 0x73, 0x65 } };
        }

        public static IEnumerable<object[]> NullableTestCases()
        {
            yield return new object[] { null, Array.Empty<byte>() };
        }

        public static IEnumerable<object[]> ParseExtraTestCases()
        {
            yield return new object[] { true, new byte[] { 0x54, 0x72 }, new byte[] { 0x75, 0x65 } };
        }
    }
}