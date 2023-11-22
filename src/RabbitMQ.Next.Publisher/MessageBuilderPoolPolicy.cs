using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Publisher;

internal class MessageBuilderPoolPolicy : PooledObjectPolicy<MessageBuilder>
{
    private readonly string exchange;

    public MessageBuilderPoolPolicy(string exchange)
    {
        this.exchange = exchange;
    }

    public override MessageBuilder Create() => new(this.exchange);

    public override bool Return(MessageBuilder obj)
    {
        obj.Reset();
        return true;
    }
}