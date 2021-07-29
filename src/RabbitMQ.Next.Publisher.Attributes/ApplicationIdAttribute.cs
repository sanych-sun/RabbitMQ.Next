using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ApplicationIdAttribute : MessageAttributeBase
    {
        public ApplicationIdAttribute(string applicationId)
        {
            this.ApplicationId = applicationId;
        }

        public string ApplicationId { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ApplicationId))
            {
                message.ApplicationId = this.ApplicationId;
            }
        }
    }
}