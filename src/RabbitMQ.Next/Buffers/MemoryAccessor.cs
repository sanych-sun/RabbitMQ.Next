using System;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryAccessor : IMemoryAccessor
{
    private readonly byte[] memory;

    public MemoryAccessor(byte[] memory)
    {
        ArgumentNullException.ThrowIfNull(memory);
        this.memory = memory;
    }

    public void Dispose()
    {
    }

    public int Size 
        => this.memory.Length;

    public ReadOnlyMemory<byte> Memory 
        => this.memory;

    public IMemoryAccessor Next
        => null;

    public IMemoryAccessor Append(IMemoryAccessor next) 
        => throw new NotSupportedException();
}
