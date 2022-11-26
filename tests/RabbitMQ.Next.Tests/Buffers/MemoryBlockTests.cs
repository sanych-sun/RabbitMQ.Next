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

        Assert.Equal(size, memoryBlock.Buffer.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CreateBufferThrownOnWrongSize(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryBlock(size));
    }

    [Fact]
    public void CanReset()
    {
        var memory = new MemoryBlock(10);
        memory.Slice(5, 2);
        memory.Append(new MemoryBlock(5));
        
        
        memory.Reset();
        
        Assert.True(memory.Length == memory.Buffer.Length);
        Assert.Null(memory.Next);
    }
    
    [Theory]
    [InlineData(0, 5, new byte[] {0x01, 0x02, 0x03, 0x04, 0x05}, new byte[] {0x01, 0x02, 0x03, 0x04, 0x05})]
    [InlineData(0, 3, new byte[] {0x01, 0x02, 0x03, 0x04, 0x05}, new byte[] {0x01, 0x02, 0x03})]
    [InlineData(2, 2, new byte[] {0x01, 0x02, 0x03, 0x04, 0x05}, new byte[] {0x03, 0x04})]
    [InlineData(2, 3, new byte[] {0x01, 0x02, 0x03, 0x04, 0x05}, new byte[] {0x03, 0x04, 0x05})]
    public void CanSlice(int start, int length, byte[] buffer, byte[] expected)
    {
        var memory = new MemoryBlock(buffer.Length);
        for (var i = 0; i < buffer.Length; i++)
        {
            memory.Buffer[i] = buffer[i];
        }
        
        memory.Slice(start, length);
        Assert.Equal(expected, ((ReadOnlyMemory<byte>)memory).ToArray());
    }

    [Theory]
    [InlineData(10, -5, 0)]
    [InlineData(10, 0, -5)]
    [InlineData(10, 0, 15)]
    [InlineData(10, 5, 7)]
    public void SliceThrowsOnInvalidParameters(int bufferSize, int start, int length)
    {
        var memory = new MemoryBlock(bufferSize);
        Assert.Throws<ArgumentOutOfRangeException>(() => memory.Slice(start, length));
    }

    [Fact]
    public void CanAppendSingleBlock()
    {
        var memory = new MemoryBlock(10);
        var next = new MemoryBlock(12);

        var res = memory.Append(next);
        
        Assert.Equal(next, memory.Next);
        Assert.Equal(next, res);
    }
    
    [Fact]
    public void CanAppendMultipleBlock()
    {
        var memory = new MemoryBlock(10);
        var next = new MemoryBlock(12);
        var last = new MemoryBlock(14);
        next.Append(last);

        var res = memory.Append(next);
        
        Assert.Equal(next, memory.Next);
        Assert.Equal(last, res);
    }
}