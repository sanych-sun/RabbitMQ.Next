using System;
using System.Collections.Generic;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers
{
    public class MemoryBlockTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void CreateBufferBySize(int size)
        {
            var memoryBlock = new MemoryBlock(size);

            Assert.Equal(size, memoryBlock.Writer.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void CreateBufferThrownOnWrongSize(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryBlock(size));
        }

        [Theory]
        [MemberData(nameof(DataWriterTestCases))]
        public void CanCommitData(byte[] expected, byte[][] data)
        {
            var memoryBlock = new MemoryBlock(100);

            foreach (var part in data)
            {
                part.CopyTo(memoryBlock.Writer);
                memoryBlock.Commit(part.Length);
            }

            Assert.Equal(expected, memoryBlock.Memory.ToArray());
        }

        [Fact]
        public void CanResetWrittenBytes()
        {
            var memoryBlock = new MemoryBlock(10);

            Assert.Equal(0, memoryBlock.Memory.Length);
            Assert.Equal(10, memoryBlock.Writer.Length);

            memoryBlock.Commit(3);
            Assert.Equal(3, memoryBlock.Memory.Length);
            Assert.Equal(7, memoryBlock.Writer.Length);

            memoryBlock.Reset();
            Assert.Equal(0, memoryBlock.Memory.Length);
            Assert.Equal(10, memoryBlock.Writer.Length);
        }

        public static IEnumerable<object[]> DataWriterTestCases()
        {
            yield return new object[] { new byte[] { 0x01, 0x02, 0x03 }, new[] { new byte[] { 0x01, 0x02, 0x03 } } };
            yield return new object[] { new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }, new[] { new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x04, 0x05 } } };
        }
    }
}