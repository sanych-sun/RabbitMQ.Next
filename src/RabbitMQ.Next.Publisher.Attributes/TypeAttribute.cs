using System;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TypeAttribute : MessageAttributeBase
{
    public TypeAttribute(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentNullException(nameof(type));
        }

        this.Type = type;
    }

    public string Type { get; }

    public override void Apply(IMessageBuilder message)
        => message.SetType(this.Type);
}