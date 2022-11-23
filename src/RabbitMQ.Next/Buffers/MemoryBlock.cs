using System;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryBlock
{
    public readonly byte[] Buffer;
    private int offset;
    private int count;

    public MemoryBlock(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        this.Buffer = new byte[size];
    }
    public MemoryBlock Next { get; private set; }

    public bool Reset()
    {
        this.offset = 0;
        this.count = this.Buffer.Length;
        this.Next = null;
        return true;
    }

    public MemoryBlock Append(MemoryBlock next)
    {
        this.Next = next;
        while (next.Next != null)
        {
            next = next.Next;
        }
        
        return next;
    }

    public void Slice(int start, int length)
    {
        if (start < 0 || start > this.Buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (length < 0 || start + length > this.Buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        this.offset = start;
        this.count = length;
    }

    public ArraySegment<byte> Data => new (this.Buffer, this.offset, this.count);
}