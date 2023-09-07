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

    public SharedMemory(ObjectPool<byte[]> memoryPool, byte[] memory)
    {
        ArgumentNullException.ThrowIfNull(memoryPool);
        ArgumentNullException.ThrowIfNull(memory);
        
        this.memoryPool = memoryPool;
        this.memory = memory;
    }

    public void Dispose()
    {
        var refsCount = Interlocked.Decrement(ref this.referencesCount);
        if (refsCount != 0)
        {
            return;
        }
        
        this.memoryPool.Return(this.memory);
        this.memory = null;
    }
    
    public MemoryAccessor Slice(int offset, int size) 
        => new(this, offset, size);

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

        public MemoryAccessor(SharedMemory owner, int offset, int length)
        {
            ArgumentNullException.ThrowIfNull(owner);
            owner.DisposeCheck();
            
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset > owner.memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        
            if(offset + length > owner.memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            
            this.owner = owner;
            this.offset = offset;
            this.Length = length;
        }

        public int Length { get; }
        
        public ReadOnlySpan<byte> Span
        {
            get
            {
                this.owner.DisposeCheck();
                return new (this.owner.memory, this.offset, this.Length);
            }
        }

        public MemoryAccessor Slice(int offset)
            => this.Slice(offset, this.Length - offset);
        
        public MemoryAccessor Slice(int offset, int length)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            
            if (offset + length > this.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new(this.owner, this.offset + offset, length);
        }

        public IMemoryAccessor AsRef()
        {
            this.owner.DisposeCheck();
            return new SharedMemoryAccessor(this.owner, this.offset, this.Length);
        }
    }
    
    private sealed class SharedMemoryAccessor : IMemoryAccessor
    {
        private readonly int offset;
        private SharedMemory owner;

        public SharedMemoryAccessor(SharedMemory owner, int offset, int size)
        {
            this.owner = owner;
            this.offset = offset;
            this.Size = size;
            
            Interlocked.Increment(ref this.owner.referencesCount);
        }


        public void Dispose()
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
                return new(this.owner.memory, this.offset, this.Size);
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

        public void CopyTo(Span<byte> destination)
        {
            this.CheckDisposed();
            this.Memory.Span.CopyTo(destination);
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