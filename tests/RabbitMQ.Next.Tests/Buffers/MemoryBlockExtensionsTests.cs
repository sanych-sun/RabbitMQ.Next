using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers;

public class MemoryBlockExtensionsTests
{
    [Theory]
    [MemberData(nameof(WriteTestCases))]
    internal void WriteTests(byte[] data)
    {
        var memory = new MemoryBlock(100);
        memory.Write(data);

        Assert.Equal(data?.Length ?? 0, memory.Memory.Count);
        Assert.Equal(data ?? Array.Empty<byte>(), memory.Memory.ToArray());
    }
        
    [Fact]
    internal void WriteThrows()
    {
        var memory = new MemoryBlock(2);
        Assert.Throws<OutOfMemoryException>(() => memory.Write(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
    }
        
    [Theory]
    [MemberData(nameof(ToSequenceTestCases))]
    internal void ToSequenceTests(byte[] expected, MemoryBlock memory)
    {
        var result = memory.ToSequence();

        Assert.Equal(expected, result.ToArray());
    }

    public static IEnumerable<object[]> WriteTestCases()
    {
        yield return new object[] { null };
        yield return new object[] { Array.Empty<byte>() };
        yield return new object[] { new byte[] { 0x01 } };
        yield return new object[] { new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 } };
    }

    public static IEnumerable<object[]> ToSequenceTestCases()
    {
        yield return new object[]
        {
            Array.Empty<byte>(),
            null
        };
            
        yield return new object[]
        {
            Array.Empty<byte>(),
            BuildMemory(new byte[] { })
        };

        yield return new object[]
        {
            new byte[] { 0x01 },
            BuildMemory(new byte[] { 0x01 })
        };

        yield return new object[]
        {
            new byte[] { 0x01, 0x02, 0x03 },
            BuildMemory ( new byte[] { 0x01 }, new byte[] { 0x02, 0x03 } )
        };

        yield return new object[]
        {
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 },
            BuildMemory ( new byte[] { 0x01 }, new byte[] { 0x02, 0x03 }, new byte[] { 0x04, 0x05 } )
        };
    }

    private static MemoryBlock BuildMemory(params byte[][] chunks)
    {
        var first = new MemoryBlock(100);
        if (chunks == null || chunks.Length == 0)
        {
            first.Slice(0);
            return first;
        }

        first.Write(chunks[0]);
        var current = first;

        for (var i = 1; i < chunks.Length; i++)
        {
            var next = new MemoryBlock(100);
            next.Write(chunks[i]);
            
            current = current.Append(next);
        }

        return first;
    }
}