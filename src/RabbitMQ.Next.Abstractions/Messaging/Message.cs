namespace RabbitMQ.Next.Abstractions.Messaging
{
    public class Message<TContent>
    {
        public Message(string exchange, TContent content, string routingKey, MessageProperties properties)
        {
            this.Exchange = exchange;
            this.Content = content;
            this.RoutingKey = routingKey;
            this.Properties = properties;
        }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public MessageProperties Properties { get; }

        public TContent Content { get; }
    }
}