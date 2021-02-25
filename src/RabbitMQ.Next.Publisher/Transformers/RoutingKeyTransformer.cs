using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class RoutingKeyTransformer : IMessageTransformer
    {
        private readonly string routingKet;

        public RoutingKeyTransformer(string routingKey)
        {
            this.routingKet = routingKey;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.RoutingKey))
            {
                message.RoutingKey = this.routingKet;
            }
        }
    }
}