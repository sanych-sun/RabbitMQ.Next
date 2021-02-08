using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExpirationAttribute : Attribute
    {
        public ExpirationAttribute(int seconds)
        {
            this.Expiration = TimeSpan.FromSeconds(seconds);
        }

        public TimeSpan Expiration { get; }
    }
}