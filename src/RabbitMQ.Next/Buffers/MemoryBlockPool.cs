using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal class MemoryBlockPool : DefaultObjectPool<MemoryBlock>
{
    public MemoryBlockPool(int bufferSize, int maximumRetained)
        : base(new MemoryBlockPoolPolicy(bufferSize), maximumRetained)
    {
    }

    public override void Return(MemoryBlock obj)
    {
        var current = obj;
            
        while(current != null)
        {
            var next = current.Next;
            base.Return(current);
            current = next;
        }
    }
    
    private class MemoryBlockPoolPolicy: PooledObjectPolicy<MemoryBlock>
    {
        private readonly int bufferSize;

        public MemoryBlockPoolPolicy(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }
            
        public override MemoryBlock Create() => new (this.bufferSize);

        public override bool Return(MemoryBlock obj) 
            => obj.Buffer.Length == this.bufferSize && obj.Reset();
    }
}