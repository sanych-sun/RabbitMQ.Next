using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class DeliveryModeAttribute : Attribute
    {
        public DeliveryModeAttribute(DeliveryMode deliveryMode)
        {
            this.DeliveryMode = deliveryMode;
        }

        public DeliveryMode DeliveryMode { get; }
    }
}