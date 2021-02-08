using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class DeliveryModeTransformer : IMessageTransformer
    {
        private readonly DeliveryMode deliveryMode;

        public DeliveryModeTransformer(DeliveryMode deliveryMode)
        {
            this.deliveryMode = deliveryMode;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (header.Properties.DeliveryMode == DeliveryMode.Unset)
            {
                header.Properties.DeliveryMode = this.deliveryMode;
            }
        }
    }
}