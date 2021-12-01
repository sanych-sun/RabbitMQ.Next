using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
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

            var result = formatter.TryFormat(content, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(ushort? content, byte[] expected)
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            var result = formatter.TryFormat(content, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(ushort expected, params byte[][] parts)
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out ushort parsed);

            Assert.True(result);
            Assert.Equal(expected, parsed);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(ushort? expected, params byte[][] parts)
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out ushort? parsed);

            Assert.True(result);
            Assert.Equal(expected, parsed);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new UInt16Formatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() => formatter.TryParse(sequence, out ushort _));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.TryFormat((ushort)42, bufferWriter));
        }

        [Fact]
        public void ShouldNotFormatWrongType()
        {
            var formatter = new UInt16Formatter();
            var bufferWriter = Substitute.For<IBufferWriter<byte>>();

            var result = formatter.TryFormat("hello", bufferWriter);

            Assert.False(result);
            bufferWriter.DidNotReceive().Advance(Arg.Any<int>());
        }

        [Fact]
        public void ShouldNotParseToWrongType()
        {
            var formatter = new UInt16Formatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x54 });

            var result = formatter.TryParse<string>(sequence, out var _);

            Assert.False(result);
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