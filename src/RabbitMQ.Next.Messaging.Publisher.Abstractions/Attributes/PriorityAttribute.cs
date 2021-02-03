using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        public PriorityAttribute(byte priority)
        {
            this.Priority = priority;
        }

        public byte Priority { get; }
    }
}