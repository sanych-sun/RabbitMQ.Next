using System;
using System.Diagnostics.CodeAnalysis;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryBlock
{
    private readonly byte[] memory;
    private int size;

    public MemoryBlock(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        this.memory = new byte[size];
        this.size = size;
    }

    public MemoryBlock Next { get; set; }
    
    public ArraySegment<byte> Memory 
        => new (this.memory, 0, this.size);

    public bool Reset()
    {
        this.size = this.memory.Length;
        this.Next = null;
        return true;
    }
    
    public void Slice(int len)
    {
        this.size = len;
    }

}