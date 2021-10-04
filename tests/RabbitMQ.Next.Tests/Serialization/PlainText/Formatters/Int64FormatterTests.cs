using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.PlainText.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Formatters
{
    public class Int64FormatterTests
    {
        [Theory]
        [InlineData(0, new byte[] { 0x30 })]
        [InlineData(1, new byte[] { 0x31 })]
        [InlineData(42, new byte[] { 0x34, 0x32 })]
        [InlineData(-42, new byte[] { 0x2D, 0x34, 0x32 })]
        public void CanFormat(long content, byte[] expected)
        {
            var formatter = new Int64Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(expected.Length);

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0x30 })]
        [InlineData(1, new byte[] { 0x31 })]
        [InlineData(42, new byte[] { 0x34, 0x32 })]
        [InlineData(-42, new byte[] { 0x2D, 0x34, 0x32 })]
        [InlineData(-42, new byte[] { 0x2D }, new byte[] { 0x34, 0x32 })]
        public void CanParse(long expected, params byte[][] parts)
        {
            var formatter = new Int64Formatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<long>(sequence);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        [InlineData(new byte[] { 0x34, 0x32, 0x68, 0x65, 0x6C, 0x6C, 0x6F } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new Int64Formatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<FormatException>(() =>  formatter.Parse<long>(sequence));
        }

        [Theory]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), true)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(DateTimeOffset), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new Int64Formatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new Int64Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<ArgumentException>(() => formatter.Format("hello", bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new Int64Formatter();
            var sequence = Helpers.MakeSequence(new byte[] { 0x31 });

            Assert.Throws<ArgumentException>(() => formatter.Parse<string>(sequence));
        }
    }
}