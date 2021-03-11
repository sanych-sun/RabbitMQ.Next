using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Formatters;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.Formatters
{
    public class StringFormatterTests
    {
        [Theory]
        [MemberData(nameof(FormatTestCases))]
        public void CanFormat(string data, int initialBufferSize, byte[] expected)
        {
            var formatter = new StringTypeFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(initialBufferSize);

            formatter.Format(data, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(ParseTestCases))]
        public void CanParse(string expected, ReadOnlyMemory<byte> content, params int[] parts)
        {
            var formatter = new StringTypeFormatter();
            var sequence = Helpers.MakeSequence(content, parts);

            var result = formatter.Parse<string>(sequence);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(char[]), false)]
        [InlineData(typeof(char), false)]
        [InlineData(typeof(int), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new StringTypeFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new StringTypeFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<InvalidOperationException>(() => formatter.Format(42, bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new StringTypeFormatter();
            var sequence = ReadOnlySequence<byte>.Empty;

            Assert.Throws<InvalidOperationException>(() => formatter.Parse<int>(sequence));
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
                yield return new object[] { text.Text, text.Bytes };
                if (text.Bytes.Length > 50)
                {
                    yield return new object[] { text.Text, text.Bytes, 50 };
                }
            }
        }
    }
}