using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
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
                message.Type = this.Type;
            }
        }
    }
}