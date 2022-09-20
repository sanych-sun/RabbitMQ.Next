using System;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers;

public class MemoryBlockTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void CreateBufferBySize(int size)
    {
        var memoryBlock = new MemoryBlock(size);

        Assert.Equal(size, memoryBlock.Memory.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CreateBufferThrownOnWrongSize(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryBlock(size));
    }

    [Theory]
    [InlineData(100, 0)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    public void CanSliceData(int bufferSize, int sliceSize)
    {
        var memoryBlock = new MemoryBlock(bufferSize);
        memoryBlock.Slice(sliceSize);

        Assert.Equal(sliceSize, memoryBlock.Memory.Count);
    }

    [Fact]
    public void CanReset()
    {
        var memoryBlock = new MemoryBlock(10);
        memoryBlock.Slice(5);

        var nextBlock = new MemoryBlock(12);
        memoryBlock.Next = nextBlock;
        
        Assert.Equal(5, memoryBlock.Memory.Count);
        Assert.Equal(nextBlock, memoryBlock.Next);

        memoryBlock.Reset();
        Assert.Equal(10, memoryBlock.Memory.Count);
        Assert.Null(memoryBlock.Next);
    }
}