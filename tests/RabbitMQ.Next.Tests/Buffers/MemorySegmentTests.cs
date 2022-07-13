using System;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers;

public class MemorySegmentTests
{
    [Fact]
    public void CanCreate()
    {
        var data = new byte[] { 0x01, 0x02 };

        var segment = new MemorySegment<byte>(data);

        Assert.Equal(data, segment.Memory.ToArray());
        Assert.Null(segment.Next);
        Assert.Equal(0, segment.RunningIndex);
    }

    [Fact]
    public void CanAppend()
    {
        var data1 = new byte[] { 0x01, 0x02 };
        var data2 = new byte[] { 0x03, 0x04, 0x05 };

        var segment1 = new MemorySegment<byte>(data1);
        var segment2 = segment1.Append(data2);

        Assert.Equal(data1, segment1.Memory.ToArray());
        Assert.Equal(segment2, segment1.Next);
        Assert.Equal(0, segment1.RunningIndex);

        Assert.Equal(data2, segment2.Memory.ToArray());
        Assert.Null(segment2.Next);
        Assert.Equal(data1.Length, segment2.RunningIndex);
    }

    [Fact]
    public void CanSkipEmptySegment()
    {
        var data1 = new byte[] { 0x01, 0x02 };

        var segment1 = new MemorySegment<byte>(data1);
        var segment2 = segment1.Append(ReadOnlyMemory<byte>.Empty);

        Assert.Equal(segment1, segment2);
        Assert.Null(segment1.Next);
        Assert.Equal(0, segment1.RunningIndex);
    }
}