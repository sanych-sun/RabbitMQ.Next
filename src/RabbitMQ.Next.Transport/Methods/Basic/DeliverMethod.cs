using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

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

        public uint Method => (uint) MethodId.BasicDeliver;

        public string Exchange { get; }

        public string RoutingKey { get; }

        public string ConsumerTag { get; }

        public ulong DeliveryTag { get; }

        public bool Redelivered { get; }
    }
}