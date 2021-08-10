using System;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class DeliveryModeInitializer : IMessageInitializer
    {
        private readonly DeliveryMode deliveryMode;

        public DeliveryModeInitializer(DeliveryMode deliveryMode)
        {
            if (deliveryMode == DeliveryMode.Unset)
            {
                throw new ArgumentOutOfRangeException(nameof(deliveryMode));
            }

            this.deliveryMode = deliveryMode;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.DeliveryMode(this.deliveryMode);
    }
}