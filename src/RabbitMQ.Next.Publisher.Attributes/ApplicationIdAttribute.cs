using System;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ApplicationIdAttribute : MessageAttributeBase
    {
        public ApplicationIdAttribute(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentNullException(nameof(applicationId));
            }

            this.ApplicationId = applicationId;
        }

        public string ApplicationId { get; }

        public override void Apply(IMessageBuilder message)
            => message.ApplicationId(this.ApplicationId);
    }
}