using System;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers
{
    public class StaticMemoryBlockTests
    {
        [Fact]
        public void CreateBufferBySize()
        {
            var data = new byte[] { 0x01, 0x02, 0x03 };
            var memoryBlock = new StaticMemoryBlock(data);

            Assert.Equal(data, memoryBlock.Memory.ToArray());
        }

        [Fact]
        public void ShouldNotReset()
        {
            var data = new byte[] { 0x01, 0x02, 0x03 };
            var memoryBlock = new StaticMemoryBlock(data);

            var res = memoryBlock.Reset();
            Assert.False(res);
            Assert.Equal(data, memoryBlock.Memory.ToArray());
        }

        [Fact]
        public void ImplicitFromArray()
        {
            var data = new byte[] { 0x01 };
            StaticMemoryBlock memoryBlock = data;

            Assert.Equal(data, memoryBlock.Memory.ToArray());
        }

        [Fact]
        public void ImplicitFromMemory()
        {
            ReadOnlyMemory<byte> data = new byte[] { 0x01 };
            StaticMemoryBlock memoryBlock = data;

            Assert.Equal(data.ToArray(), memoryBlock.Memory.ToArray());
        }
    }
}