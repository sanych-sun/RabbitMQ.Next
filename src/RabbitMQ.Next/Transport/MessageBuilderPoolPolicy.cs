using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Transport;

internal class MessageBuilderPoolPolicy : PooledObjectPolicy<MessageBuilder>
{
    private readonly ObjectPool<byte[]> memoryPool;
    private readonly ushort channel;
    private readonly int frameMaxSize;

    public MessageBuilderPoolPolicy(ObjectPool<byte[]> memoryPool, ushort channel, int frameMaxSize)
    {
        this.memoryPool = memoryPool;
        this.channel = channel;
        this.frameMaxSize = frameMaxSize;
    }
            
    public override MessageBuilder Create() => new (this.memoryPool, this.channel, this.frameMaxSize);

    public override bool Return(MessageBuilder obj)
    {
        if (this.channel != obj.Channel || this.frameMaxSize != obj.MaxFrameSize)
        {
            return false;
        }
        
        obj.Reset();
        return true;
    }
}