using System;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ReplyToAttribute : MessageAttributeBase
    {
        public ReplyToAttribute(string replyTo)
        {
            if (string.IsNullOrWhiteSpace(replyTo))
            {
                throw new ArgumentNullException(nameof(replyTo));
            }

            this.ReplyTo = replyTo;
        }

        public string ReplyTo { get; }

        public override void Apply(IMessageBuilder message)
            => message.ReplyTo(this.ReplyTo);
    }
}