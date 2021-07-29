using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Publisher
{
    internal class MessageBuilderPoolPolicy : PooledObjectPolicy<MessageBuilder>
    {
        public override MessageBuilder Create() => new();

        public override bool Return(MessageBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}