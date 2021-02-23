using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    public abstract class MessageAttributeBase : Attribute
    {
        public abstract void Apply(IMessageBuilder message);
    }
}