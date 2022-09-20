using System;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers;

public class MemoryBlockPoolPolicyTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(12345)]
    public void CanCreate(int size)
    {
        var policy = new MemoryBlockPoolPolicy(size);
        var buffer = policy.Create();

        Assert.Equal(size, buffer.Memory.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ThrowsOnWrongSize(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryBlockPoolPolicy(size));
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    public void ShouldAcceptBufferWithProperSize(int policySize, int bufferSize)
    {
        var policy = new MemoryBlockPoolPolicy(policySize);
        var buffer = new MemoryBlock(bufferSize);

        var result = policy.Return(buffer);
        
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(100, 10)]
    [InlineData(10, 100)]
    public void ShouldNotAcceptBufferWithWrongSize(int policySize, int bufferSize)
    {
        var policy = new MemoryBlockPoolPolicy(policySize);
        var buffer = new MemoryBlock(bufferSize);

        var result = policy.Return(buffer);
        
        Assert.False(result);
    }
}