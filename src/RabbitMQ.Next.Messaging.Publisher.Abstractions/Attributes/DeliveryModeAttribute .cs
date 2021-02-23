using System;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class DeliveryModeAttribute : MessageAttributeBase
    {
        public DeliveryModeAttribute(DeliveryMode deliveryMode)
        {
            this.DeliveryMode = deliveryMode;
        }

        public DeliveryMode DeliveryMode { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (message.DeliveryMode == DeliveryMode.Unset)
            {
                message.SetDeliveryMode(message.DeliveryMode);
            }
        }
    }
}