using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using IMessageBuilder = RabbitMQ.Next.Publisher.Abstractions.Transformers.IMessageBuilder;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class DeliveryModeTransformer : IMessageTransformer
    {
        private readonly DeliveryMode deliveryMode;

        public DeliveryModeTransformer(DeliveryMode deliveryMode)
        {
            this.deliveryMode = deliveryMode;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (message.DeliveryMode == DeliveryMode.Unset)
            {
                message.SetDeliveryMode(this.deliveryMode);
            }
        }
    }
}