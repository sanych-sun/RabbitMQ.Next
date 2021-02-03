using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ApplicationIdAttribute : Attribute
    {
        public ApplicationIdAttribute(string applicationId)
        {
            this.ApplicationId = applicationId;
        }

        public string ApplicationId { get; }
    }
}