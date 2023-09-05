using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
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

        Assert.Equal(data?.Length ?? 0, memory.Length);
        Assert.Equal(data ?? Array.Empty<byte>(), ((ReadOnlyMemory<byte>)memory).ToArray());
    }
        
    [Fact]
    internal void WriteThrows()
    {
        var memory = new MemoryBlock(2);
        Assert.Throws<OutOfMemoryException>(() => memory.Write(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
    }
        
    [Theory]
    [MemberData(nameof(ToSequenceTestCases))]
    internal void ToSequenceTests(byte[] expected, byte[][] data)
    {
        var memory = BuildMemory(data);
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
            new[] { Array.Empty<byte>()}
        };

        yield return new object[]
        {
            new byte[] { 0x01 },
            new[] { new byte[] { 0x01 }}
        };

        yield return new object[]
        {
            new byte[] { 0x01, 0x02, 0x03 },
            new[] { new byte[] { 0x01 }, new byte[] { 0x02, 0x03 } }
        };

        yield return new object[]
        {
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 },
            new[] { new byte[] { 0x01 }, new byte[] { 0x02, 0x03 }, new byte[] { 0x04, 0x05 } }
        };
    }

    private static IMemoryAccessor BuildMemory(params byte[][] chunks)
    {
        if (chunks == null || chunks.Length == 0)
        {
            return new StaticMemoryAccessor(Array.Empty<byte>());
        }
            
        
        
        var first = SubstituteMemoryAccessor(chunks[0]);
        IMemoryAccessor current = first;

        for (var i = 1; i < chunks.Length; i++)
        {
            var next = SubstituteMemoryAccessor(chunks[i]);
            current = current.Append(next);
        }

        return first;
    }

    private static IMemoryAccessor SubstituteMemoryAccessor(byte[] memory)
    {
        var subject = Substitute.For<IMemoryAccessor>();
        subject.Memory.Returns(memory);
        subject.Size.Returns(memory.Length);
        subject.Next.Returns((IMemoryAccessor)null);
        subject.Append(Arg.Any<IMemoryAccessor>())
            .Returns(x => x[0])
            .AndDoes(x =>
            {
                subject.Next.Returns(x[0]);
            });

        return subject;
    }
}