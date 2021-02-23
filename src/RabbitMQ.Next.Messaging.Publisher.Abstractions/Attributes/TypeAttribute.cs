using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeAttribute : MessageAttributeBase
    {
        public TypeAttribute(string type)
        {
            this.Type = type;
        }

        public string Type { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.Type))
            {
                message.SetType(this.Type);
            }
        }
    }
}