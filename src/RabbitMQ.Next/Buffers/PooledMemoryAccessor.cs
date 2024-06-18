using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal sealed class PooledMemoryAccessor : IMemoryAccessor
{
    private readonly ObjectPool<byte[]> memoryPool;
    private byte[] memory;
    private readonly int offset;

    public PooledMemoryAccessor(ObjectPool<byte[]> memoryPool, byte[] memory, int offset, int size)
    {
        ArgumentNullException.ThrowIfNull(memoryPool);
        ArgumentNullException.ThrowIfNull(memory);
        
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (offset > memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        
        if(offset + size > memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }
        
        this.memoryPool = memoryPool;
        this.memory = memory;
        this.offset = offset;
        this.Size = size;
    }

    ~PooledMemoryAccessor() => this.ReleaseMemory();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.ReleaseMemory();
    }

    private void ReleaseMemory()
    {
        if (this.memory == null)
        {
            return;
        }
        
        this.memoryPool.Return(this.memory);
        this.memory = null;
        this.Next = null;
    }
    
    public int Size { get; }

    public ReadOnlyMemory<byte> Memory
    {
        get
        {
            this.CheckDisposed();
            return new ReadOnlyMemory<byte>(this.memory, this.offset, this.Size);
        }
    }

    public IMemoryAccessor Next { get; private set; }

    public IMemoryAccessor Append(IMemoryAccessor next)
    {
        if (this.Next != null)
        {
            throw new InvalidOperationException();
        }

        this.Next = next;
        return next;
    }

    public void WriteTo(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        this.CheckDisposed();
        
        stream.Write(this.memory, this.offset, this.Size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckDisposed()
    {
        if (this.memory == null)
        {
            throw new ObjectDisposedException(nameof(PooledMemoryAccessor));
        }
    }
}
