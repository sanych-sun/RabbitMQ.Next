using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExpirationAttribute : Attribute
    {
        public ExpirationAttribute(TimeSpan expiration)
        {
            this.Expiration = expiration;
        }

        public TimeSpan Expiration { get; }
    }
}