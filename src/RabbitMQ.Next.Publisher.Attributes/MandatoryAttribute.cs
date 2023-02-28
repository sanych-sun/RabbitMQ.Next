using System;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class MandatoryAttribute : MessageAttributeBase
{
    public override void Apply(IMessageBuilder message)
        => message.SetMandatory();
}