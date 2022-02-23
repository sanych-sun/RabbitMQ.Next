namespace RabbitMQ.Next.Consumer
{
    public readonly struct DeliveredMessage
    {
        public DeliveredMessage(string exchange, string routingKey, bool redelivered, string consumerTag, ulong deliveryTag)
        {
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
            this.Redelivered = redelivered;
            this.ConsumerTag = consumerTag;
            this.DeliveryTag = deliveryTag;
        }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public bool Redelivered { get; }

        public string ConsumerTag { get; }

        public ulong DeliveryTag { get; }
    }
}