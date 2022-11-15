using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal class MemoryBlockPoolPolicy: PooledObjectPolicy<MemoryBlock>
{
    private readonly int bufferSize;

    public MemoryBlockPoolPolicy(int bufferSize)
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        
        this.bufferSize = bufferSize;
    }
            
    public override MemoryBlock Create() => new (this.bufferSize);

    public override bool Return(MemoryBlock obj)
    {
        if (this.bufferSize != obj.Memory.Array.Length)
        {
            return false;
        }
        
        return obj.Reset();
    }
}