using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class UInt16FormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(ushort content, byte[] expected)
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(ushort? content, byte[] expected)
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(ushort expected, params byte[][] parts)
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<ushort>(sequence);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(ushort? expected, params byte[][] parts)
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<ushort?>(sequence);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new UInt16Formatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<ushort>(sequence));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.Format((ushort)42, bufferWriter));
        }

        [Theory]
        [InlineData(typeof(ushort), true)]
        [InlineData(typeof(ushort?), true)]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(DateTimeOffset), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new UInt16Formatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x31 });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
        }

        public static IEnumerable<object[]> GenericTestCases()
        {
            yield return new object[] { (ushort)0, new byte[] { 0x30 } };
            yield return new object[] { (ushort)1, new byte[] { 0x31 } };
            yield return new object[] { (ushort)42, new byte[] { 0x34, 0x32 } };
        }

        public static IEnumerable<object[]> NullableTestCases()
        {
            yield return new object[] { null, Array.Empty<byte>() };
        }

        public static IEnumerable<object[]> ParseExtraTestCases()
        {
            yield return new object[] { (ushort)42, new byte[] { 0x34 }, new byte[] { 0x32 } };
        }
    }
}