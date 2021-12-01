using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class StringFormatterTests
    {
        [Theory]
        [MemberData(nameof(FormatTestCases))]
        public void CanFormat(string content, int initialBufferSize, byte[] expected)
        {
            var formatter = new StringFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(initialBufferSize);

            var result = formatter.TryFormat(content, bufferWriter);

            Assert.True(result);
            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(ParseTestCases))]
        public void CanParse(string expected, params byte[][] parts)
        {
            var formatter = new StringFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.TryParse(sequence, out string parsed);

            Assert.True(result);
            Assert.Equal(expected, parsed);
        }

        [Fact]
        public void ShouldNotFormatWrongType()
        {
            var formatter = new StringFormatter();
            var bufferWriter = Substitute.For<IBufferWriter<byte>>();

            var result = formatter.TryFormat(42, bufferWriter);

            Assert.False(result);
            bufferWriter.DidNotReceive().Advance(Arg.Any<int>());
        }

        [Fact]
        public void ShouldNotParseToWrongType()
        {
            var formatter = new SingleFormatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x54 });

            var result = formatter.TryParse<int>(sequence, out var _);

            Assert.False(result);
        }

        public static IEnumerable<object[]> FormatTestCases()
        {
            yield return new object[] { string.Empty, 50, Array.Empty<byte>()};

            var texts = Helpers.GetDummyTexts(0, 128);

            foreach (var text in texts)
            {
                yield return new object[] { text.Text, 50, text.Bytes.ToArray() };
            }
        }

        public static IEnumerable<object[]> ParseTestCases()
        {
            yield return new object[] { string.Empty, new byte[0] };

            var texts = Helpers.GetDummyTexts(0, 128);

            foreach (var text in texts)
            {
                yield return new object[] { text.Text, text.Bytes.ToArray() };
                if (text.Bytes.Length > 50)
                {
                    yield return new object[] { text.Text, text.Bytes.Slice(0, 50).ToArray(), text.Bytes.Slice(50).ToArray() };
                }
            }
        }
    }
}