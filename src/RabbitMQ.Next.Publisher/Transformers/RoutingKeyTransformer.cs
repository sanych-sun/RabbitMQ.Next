using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class RoutingKeyTransformer : IMessageTransformer
    {
        private readonly string routingKey;

        public RoutingKeyTransformer(string routingKey)
        {
            this.routingKey = routingKey;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.RoutingKey))
            {
                message.SetRoutingKey(this.routingKey);
            }
        }
    }
}