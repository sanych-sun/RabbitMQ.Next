using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class PriorityAttribute : MessageAttributeBase
    {
        public PriorityAttribute(byte priority)
        {
            this.Priority = priority;
        }

        public byte Priority { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (!message.Priority.HasValue)
            {
                message.Priority = this.Priority;
            }
        }
    }
}