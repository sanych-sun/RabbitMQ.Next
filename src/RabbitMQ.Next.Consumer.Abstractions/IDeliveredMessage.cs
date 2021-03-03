using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public readonly struct DeliveredMessage
    {
        public DeliveredMessage(string exchange, string routingKey, IMessageProperties properties, bool redelivered, string consumerTag, ulong deliveryTag)
        {
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
            this.Properties = properties;
            this.Redelivered = redelivered;
            this.ConsumerTag = consumerTag;
            this.DeliveryTag = deliveryTag;
        }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public IMessageProperties Properties { get; }

        public bool Redelivered { get; }

        public string ConsumerTag { get; }

        public ulong DeliveryTag { get; }
    }
}