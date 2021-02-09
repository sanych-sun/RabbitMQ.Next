using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Formatters;
using RabbitMQ.Next.Transport.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.Formatters
{
    public class Int64FormatterTests
    {
        [Theory]
        [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
        [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
        [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
        [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11010110 })]
        public void CanFormat(long content, byte[] expected)
        {
            var formatter = new Int64Formatter();
            var bufferWriter = new ArrayBufferWriter<byte>(sizeof(int));

            formatter.Format(content, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
        [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
        [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
        [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11010110 })]
        [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111 }, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11010110 })]
        [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111}, new byte[] { 0b_11111111, 0b_11111111}, new byte[] { 0b_11111111, 0b_11010110 })]
        public void CanParse(long expected, params byte[][] contentparts)
        {
            var formatter = new Int64Formatter();
            var segment = new MemorySegment<byte>(contentparts[0]);
            var firstSegment = segment;
            for (var i = 1; i < contentparts.Length; i++)
            {
                segment = segment.Append(contentparts[i]);
            }

            var sequence = new ReadOnlySequence<byte>(firstSegment, 0, segment, segment.Memory.Length);

            var result = formatter.Parse(sequence);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1 } )]
        [InlineData(new byte[] { 1, 2, 3, 4 } )]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } )]
        public void ParseThrowsOnWrongContent(byte[] content)
        {
            var formatter = new Int64Formatter();
            var sequence = new ReadOnlySequence<byte>(content);

            Assert.Throws<ArgumentException>(() =>  formatter.Parse(sequence));
        }
    }
}