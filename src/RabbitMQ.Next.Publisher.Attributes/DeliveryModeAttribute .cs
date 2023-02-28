using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class DeliveryModeAttribute : MessageAttributeBase
{
    public DeliveryModeAttribute(DeliveryMode deliveryMode)
    {
        if (deliveryMode == DeliveryMode.Unset)
        {
            throw new ArgumentOutOfRangeException(nameof(deliveryMode));
        }

        this.DeliveryMode = deliveryMode;
    }

    public DeliveryMode DeliveryMode { get; }

    public override void Apply(IMessageBuilder message)
        => message.SetDeliveryMode(this.DeliveryMode);
}