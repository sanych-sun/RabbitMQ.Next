using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct DeliverMethod : IIncomingMethod
    {
        public DeliverMethod(string exchange, string routingKey, string consumerTag, ulong deliveryTag, bool redelivered)
        {
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
            this.ConsumerTag = consumerTag;
            this.DeliveryTag = deliveryTag;
            this.Redelivered = redelivered;
        }

        public MethodId MethodId => MethodId.BasicDeliver;

        public string Exchange { get; }

        public string RoutingKey { get; }

        public string ConsumerTag { get; }

        public ulong DeliveryTag { get; }

        public bool Redelivered { get; }
    }
}