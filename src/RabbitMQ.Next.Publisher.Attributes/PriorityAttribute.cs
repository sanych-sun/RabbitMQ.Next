using System;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class PriorityAttribute : MessageAttributeBase
{
    public PriorityAttribute(byte priority)
    {
        this.Priority = priority;
    }

    public byte Priority { get; }

    public override void Apply(IMessageBuilder message)
        => message.SetPriority(this.Priority);
}