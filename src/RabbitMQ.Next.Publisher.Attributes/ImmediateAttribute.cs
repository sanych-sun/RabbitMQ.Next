using System;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class ImmediateAttribute : MessageAttributeBase
{
    public override void Apply(IMessageBuilder message)
        => message.SetImmediate();
}