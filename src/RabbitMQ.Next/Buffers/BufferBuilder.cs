using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal class BufferBuilder
{
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private MemoryBlock head;
    private MemoryBlock current;
    private int currentOffset;

    public BufferBuilder(ObjectPool<MemoryBlock> memoryPool)
    {
        this.memoryPool = memoryPool;
    }

    public void Reset()
    {
        this.head = null;
        this.current = null;
        this.currentOffset = 0;
    }

    public MemoryBlock Complete()
    {
        this.FinalizeCurrentBlock();
        var result = this.head;
        this.Reset();
        return result;
    }

    public Span<byte> GetSpan(int size)
        => this.GetSpan(size, size);

    public Span<byte> GetSpan(int minSize, int maxSize)
    {
        this.EnsureBufferSize(minSize);
        var size = Math.Min(maxSize, this.current.Size - this.currentOffset);
        return new Span<byte>(this.current.Memory.Array, this.currentOffset, size);
    }

    public Memory<byte> GetMemory(int size)
        => this.GetMemory(size, size);
    
    public Memory<byte> GetMemory(int minSize, int maxSize)
    {
        this.EnsureBufferSize(minSize);
        var size = Math.Min(maxSize, this.current.Size - this.currentOffset);
        return new Memory<byte>(this.current.Memory.Array, this.currentOffset, size);
    }

    public void Advance(int count)
    {
        this.currentOffset += count;
    }

    private void EnsureBufferSize(int minSize)
    {
        if (this.current == null)
        {
            this.current = this.memoryPool.Get();
            this.head = this.current;
            return;
        }

        if (minSize > this.current.Size - this.currentOffset)
        {
            this.FinalizeCurrentBlock();
            var next = this.memoryPool.Get();
            this.current.Next = next;
            this.current = next;
            this.currentOffset = 0;
        }
    }
    
    private void FinalizeCurrentBlock()
    {
        if (this.currentOffset > 0)
        {
            this.current.Slice(this.currentOffset);
        }
    }
}