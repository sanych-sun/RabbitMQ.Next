namespace RabbitMQ.Next.Abstractions.Messaging
{
    public class Message<TContent>
    {
        public Message(TContent content, string routingKey, MessageProperties properties)
        {
            this.Content = content;
            this.RoutingKey = routingKey;
            this.Properties = properties;
        }

        public string RoutingKey { get; }

        public MessageProperties Properties { get; }

        public TContent Content { get; }
    }
}