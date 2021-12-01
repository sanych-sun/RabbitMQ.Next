using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class DecimalFormatterTests
    {
        [Theory]
        [MemberData(nameof(GenericTestCases))]
        public void CanFormat(decimal content, byte[] expected)
        {
            var formatter = new DecimalFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            var result = formatter.TryFormat(content, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        public void CanFormatNullable(decimal? content, byte[] expected)
        {
            var formatter = new DecimalFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length == 0 ? 1 : expected.Length);

            var result = formatter.TryFormat(content, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParse(decimal expected, params byte[][] parts)
        {
            var formatter = new DecimalFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out decimal parsed);

            Assert.True(result);
            Assert.Equal(expected, parsed);
        }

        [Theory]
        [MemberData(nameof(GenericTestCases))]
        [MemberData(nameof(NullableTestCases))]
        [MemberData(nameof(ParseExtraTestCases))]
        public void CanParseNullable(decimal? expected, params byte[][] parts)
        {
            var formatter = new DecimalFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out decimal? parsed);

            Assert.True(result);
            Assert.Equal(expected, parsed);
        }


        [Theory]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new DecimalFormatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() => formatter.TryParse(sequence, out decimal _));
        }

        [Fact]
        public void ThrowsOnTooSmallBuffer()
        {
            var formatter = new DecimalFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(1);

            Assert.Throws<OutOfMemoryException>(() => formatter.TryFormat((decimal)42, bufferWriter));
        }

        [Fact]
        public void ShouldNotFormatWrongType()
        {
            var formatter = new DecimalFormatter();
            var bufferWriter = Substitute.For<IBufferWriter<byte>>();

            var result = formatter.TryFormat("hello", bufferWriter);

            Assert.False(result);
            bufferWriter.DidNotReceive().Advance(Arg.Any<int>());
        }

        [Fact]
        public void ShouldNotParseToWrongType()
        {
            var formatter = new DecimalFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x54 });

            var result = formatter.TryParse<string>(sequence, out var _);

            Assert.False(result);
        }

        public static IEnumerable<object[]> GenericTestCases()
        {
            yield return new object[] { 0m, new byte[] { 0x30 } };
            yield return new object[] { 1m, new byte[] { 0x31 } };
            yield return new object[] { 4.2m, new byte[] { 0x34, 0x2e, 0x32 } };
            yield return new object[] { -4.2m, new byte[] { 0x2D, 0x34, 0x2e, 0x32 } };
        }

        public static IEnumerable<object[]> NullableTestCases()
        {
            yield return new object[] { null, Array.Empty<byte>() };
        }

        public static IEnumerable<object[]> ParseExtraTestCases()
        {
            yield return new object[] { -4.2m, new byte[] { 0x2D, 0x34 }, new byte[] { 0x2e, 0x32 } };
        }
    }
}