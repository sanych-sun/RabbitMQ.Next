using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
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
                message.SetUserId(this.UserId);
            }
        }
    }
}