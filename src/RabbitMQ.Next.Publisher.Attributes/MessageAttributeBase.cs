using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    public abstract class MessageAttributeBase : Attribute
    {
        public abstract void Apply(IMessageBuilder message);
    }
}