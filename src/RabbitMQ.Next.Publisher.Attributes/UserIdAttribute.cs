using System;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class UserIdAttribute : MessageAttributeBase
    {
        public UserIdAttribute(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            this.UserId = userId;
        }

        public string UserId { get; }

        public override void Apply(IMessageBuilder message)
            => message.UserId(this.UserId);
    }
}