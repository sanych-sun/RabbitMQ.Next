using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Formatters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.Formatters
{
    public class ArrayFormatterTests
    {
        [Theory]
        [InlineData(1, new byte[0])]
        [InlineData(5, new byte[] { 1, 2, 3, 4, 5 })]
        [InlineData(5, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
        public void CanFormat(int initialBufferSize, byte[] data)
        {
            var formatter = new ArrayTypeFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(initialBufferSize);

            formatter.Format(data, bufferWriter);

            Assert.Equal(data, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 2 }, new byte[] { 3, 4, 5 })]
        public void CanParse(byte[] expected, params byte[][] parts)
        {
            var formatter = new ArrayTypeFormatter();
            var sequence = Helpers.MakeSequence(parts);

            var result = formatter.Parse<byte[]>(sequence);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(typeof(byte[]), true)]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(int[]), false)]
        [InlineData(typeof(string), false)]
        public void CanHandle(Type type, bool expected)
        {
            var formatter = new ArrayTypeFormatter();

            Assert.Equal(expected, formatter.CanHandle(type));
        }

        [Fact]
        public void ThrowsOnInvalidFormat()
        {
            var formatter = new ArrayTypeFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>();

            Assert.Throws<InvalidOperationException>(() => formatter.Format(new int[] {1, 2}, bufferWriter));
        }

        [Fact]
        public void ThrowsOnInvalidParse()
        {
            var formatter = new ArrayTypeFormatter();
            var sequence = ReadOnlySequence<byte>.Empty;

            Assert.Throws<InvalidOperationException>(() => formatter.Parse<int[]>(sequence));
        }
    }
}