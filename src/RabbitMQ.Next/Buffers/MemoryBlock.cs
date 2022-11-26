using System;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryBlock
{
    public readonly byte[] Buffer;

    public MemoryBlock(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        this.Buffer = new byte[size];
        this.Length = this.Buffer.Length;
    }
    
    public int Start { get; private set; }
    
    public int Length { get; private set; }

    public MemoryBlock Next { get; private set; }

    public bool Reset()
    {
        this.Start = 0;
        this.Length = this.Buffer.Length;
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

        this.Start = start;
        this.Length = length;
    }

    public static implicit operator ReadOnlyMemory<byte>(MemoryBlock memory)
        => new (memory.Buffer, memory.Start, memory.Length);
    
    public static implicit operator ReadOnlySpan<byte>(MemoryBlock memory)
        => new (memory.Buffer, memory.Start, memory.Length);
}