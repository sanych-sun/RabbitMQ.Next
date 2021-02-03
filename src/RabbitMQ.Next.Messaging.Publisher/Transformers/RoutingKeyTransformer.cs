using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class RoutingKeyTransformer : IMessageTransformer
    {
        private readonly string routingKet;

        public RoutingKeyTransformer(string routingKey)
        {
            this.routingKet = routingKey;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.RoutingKey))
            {
                header.RoutingKey = this.routingKet;
            }
        }
    }
}