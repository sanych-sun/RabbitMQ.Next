using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class UserIdAttribute : Attribute
    {
        public UserIdAttribute(string userId)
        {
            this.UserId = userId;
        }

        public string UserId { get; }
    }
}