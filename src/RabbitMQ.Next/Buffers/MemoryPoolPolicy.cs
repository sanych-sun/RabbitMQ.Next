using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers;

internal sealed class MemoryPoolPolicy : PooledObjectPolicy<byte[]>
{
    private readonly int bufferSize;

    public MemoryPoolPolicy(int bufferSize)
    {
        this.bufferSize = bufferSize;
    }

    public override byte[] Create()
        => new byte[this.bufferSize];

    public override bool Return(byte[] obj)
        => obj.Length == this.bufferSize;
}