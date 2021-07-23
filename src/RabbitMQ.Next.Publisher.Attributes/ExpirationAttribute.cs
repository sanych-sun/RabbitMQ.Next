using System;
using System.Globalization;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExpirationAttribute : MessageAttributeBase
    {
        private readonly string expirationText;

        public ExpirationAttribute(int seconds)
        {
            this.Expiration = TimeSpan.FromSeconds(seconds);
            this.expirationText = this.Expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        public TimeSpan Expiration { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.Expiration))
            {
                message.Expiration = this.expirationText;
            }
        }
    }
}