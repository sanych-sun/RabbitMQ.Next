using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class UserIdAttribute : MessageAttributeBase
    {
        public UserIdAttribute(string userId)
        {
            this.UserId = userId;
        }

        public string UserId { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.UserId))
            {
                message.UserId = this.UserId;
            }
        }
    }
}