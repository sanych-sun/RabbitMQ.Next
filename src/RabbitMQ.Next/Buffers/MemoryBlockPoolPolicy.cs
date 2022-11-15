using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal class MemoryBlockPoolPolicy: PooledObjectPolicy<MemoryBlock>
{
    private readonly int bufferSize;

    public MemoryBlockPoolPolicy(int bufferSize)
    {
        this.bufferSize = bufferSize;
    }
            
    public override MemoryBlock Create() => new (this.bufferSize);

    public override bool Return(MemoryBlock obj) => obj.Reset();
}