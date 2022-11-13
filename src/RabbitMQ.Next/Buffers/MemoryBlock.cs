using System;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryBlock
{
    private readonly byte[] memory;

    public MemoryBlock(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        this.memory = new byte[size];
        this.Size = size;
    }

    public int Size { get; private set; }
    
    public MemoryBlock Next { get; set; }
    
    public ArraySegment<byte> Memory 
        => new (this.memory, 0, this.Size);

    public bool Reset()
    {
        this.Size = this.memory.Length;
        this.Next = null;
        return true;
    }
    
    public void Slice(int len)
    {
        this.Size = len;
    }
}