using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ReplyToAttribute : Attribute
    {
        public ReplyToAttribute(string replyTo)
        {
            this.ReplyTo = replyTo;
        }

        public string ReplyTo { get; }
    }
}