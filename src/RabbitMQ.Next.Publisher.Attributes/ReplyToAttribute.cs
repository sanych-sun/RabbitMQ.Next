using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ReplyToAttribute : MessageAttributeBase
    {
        public ReplyToAttribute(string replyTo)
        {
            this.ReplyTo = replyTo;
        }

        public string ReplyTo { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ReplyTo))
            {
                message.SetReplyTo(this.ReplyTo);
            }
        }
    }
}