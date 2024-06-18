using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal sealed class SharedMemory : IDisposable
{
    private readonly ObjectPool<byte[]> memoryPool;
    private int referencesCount = 1;
    private byte[] memory;

    public SharedMemory(ObjectPool<byte[]> memoryPool, byte[] memory, int size)
    {
        ArgumentNullException.ThrowIfNull(memoryPool);
        ArgumentNullException.ThrowIfNull(memory);
        
        this.memoryPool = memoryPool;
        this.memory = memory;
        this.Size = size;
    }

    ~SharedMemory() => this.ReleaseMemory();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.ReleaseMemory();
    }

    private void ReleaseMemory()
    {
        var refsCount = Interlocked.Decrement(ref this.referencesCount);
        if (refsCount != 0)
        {
            return;
        }
        
        this.memoryPool.Return(this.memory);
        this.memory = null;
    }
    
    public int Size { get; }

    public MemoryAccessor Slice(int offset) 
        => new(this, offset, this.Size - offset);
    
    public MemoryAccessor Slice(int offset, int size) 
        => new(this, offset, size);

    public static implicit operator MemoryAccessor(SharedMemory memory)
        => new (memory, 0, memory.Size);

    private void DisposeCheck()
    {
        if (this.memory == null)
        {
            throw new ObjectDisposedException(nameof(SharedMemory));
        }
    }

    public readonly ref struct MemoryAccessor
    {
        private readonly SharedMemory owner;
        private readonly int offset;

        public MemoryAccessor(SharedMemory owner, int offset, int size)
        {
            ArgumentNullException.ThrowIfNull(owner);
            owner.DisposeCheck();
            
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset > owner.Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        
            if(offset + size > owner.Size)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            
            this.owner = owner;
            this.offset = offset;
            this.Size = size;
            this.Span = new ReadOnlySpan<byte>(this.owner.memory, offset, size);
        }

        public int Size { get; }

        public ReadOnlySpan<byte> Span { get; }

        public MemoryAccessor Slice(int offset)
            => new(this.owner, this.offset + offset, this.Size - offset);
        
        public MemoryAccessor Slice(int offset, int size) 
            => new(this.owner, this.offset + offset, size);

        public IMemoryAccessor AsRef() 
            => new SharedMemoryAccessor(this.owner, this.offset, this.Size);
    }
    
    private sealed class SharedMemoryAccessor : IMemoryAccessor
    {
        private readonly int offset;
        private SharedMemory owner;

        public SharedMemoryAccessor(SharedMemory owner, int offset, int size)
        {
            owner.DisposeCheck();
            
            this.owner = owner;
            this.offset = offset;
            this.Size = size;
            
            Interlocked.Increment(ref this.owner.referencesCount);
        }

        ~SharedMemoryAccessor() => this.ReleaseMemory();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.ReleaseMemory();
        }

        private void ReleaseMemory()
        {
            if (this.owner == null)
            {
                return;
            }
            
            this.owner.Dispose();
            this.owner = null;
            this.Next = null;
        }
        
        public int Size { get; }

        public ReadOnlyMemory<byte> Memory
        {
            get
            {
                this.CheckDisposed();
                return new ReadOnlyMemory<byte>(this.owner.memory, this.offset, this.Size);
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
        
            stream.Write(this.owner.memory, this.offset, this.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.owner == null)
            {
                throw new ObjectDisposedException(nameof(SharedMemoryAccessor));
            }
        }
    }
}
