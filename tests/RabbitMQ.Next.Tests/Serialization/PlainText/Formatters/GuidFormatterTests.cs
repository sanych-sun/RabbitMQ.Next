using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class GuidFormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(string content, byte[] expected)
        {
            var guid = Guid.Parse(content);
            var formatter = new GuidFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            formatter.Format(guid, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(string content, byte[] expected)
        {
            Guid? guid = content == null ? null : Guid.Parse(content);
            var formatter = new GuidFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            formatter.Format(guid, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(string expected, params byte[][] parts)
        {
            var guid = Guid.Parse(expected);
            var formatter = new GuidFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<Guid>(sequence);

            Assert.Equal(guid, result);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(string expected, params byte[][] parts)
        {
            Guid? guid = expected == null ? null : Guid.Parse(expected);
            var formatter = new GuidFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<Guid?>(sequence);

            Assert.Equal(guid, result);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new GuidFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<Guid>(sequence));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new GuidFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.Format(Guid.Empty, bufferWriter));
        }

        [Theory]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(Guid), true)]
        [InlineData(typeof(Guid?), true)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(DateTimeOffset), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new GuidFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new GuidFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new GuidFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x7B, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x7D });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
        }

        public static IEnumerable<object[]> GenericTestCases()
        {
            yield return new object[] { "{96d21a17-abcd-472d-a684-9fcdd33ae9e2}", new byte[] { 0x7B, 0x39, 0x36, 0x64, 0x32, 0x31, 0x61, 0x31, 0x37, 0x2D, 0x61, 0x62, 0x63, 0x64, 0x2D, 0x34, 0x37, 0x32, 0x64, 0x2D, 0x61, 0x36, 0x38, 0x34, 0x2D, 0x39, 0x66, 0x63, 0x64, 0x64, 0x33, 0x33, 0x61, 0x65, 0x39, 0x65, 0x32, 0x7D } };
            yield return new object[] { "{00000000-0000-0000-0000-000000000000}", new byte[] { 0x7B, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x7D } };
        }

        public static IEnumerable<object[]> NullableTestCases()
        {
            yield return new object[] { null, Array.Empty<byte>() };
        }

        public static IEnumerable<object[]> ParseExtraTestCases()
        {
            yield return new object[] { "{96d21a17-abcd-472d-a684-9fcdd33ae9e2}", new byte[] { 0x7B, 0x39, 0x36, 0x64, 0x32, 0x31, 0x61, 0x31, 0x37, 0x2D, 0x61, 0x62, 0x63, 0x64 }, new byte[] { 0x2D, 0x34, 0x37, 0x32, 0x64, 0x2D, 0x61, 0x36, 0x38, 0x34, 0x2D, 0x39, 0x66, 0x63, 0x64, 0x64, 0x33, 0x33, 0x61, 0x65, 0x39, 0x65, 0x32, 0x7D } };
        }
    }
}