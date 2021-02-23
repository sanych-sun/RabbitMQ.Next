using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
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
                message.SetPriority(this.Priority);
            }
        }
    }
}