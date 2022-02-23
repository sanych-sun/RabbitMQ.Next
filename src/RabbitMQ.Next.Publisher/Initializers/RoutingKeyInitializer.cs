using System;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class RoutingKeyInitializer : IMessageInitializer
    {
        private readonly string routingKey;

        public RoutingKeyInitializer(string routingKey)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentNullException(nameof(routingKey));
            }

            this.routingKey = routingKey;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.RoutingKey(this.routingKey);
    }
}