using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ContentTypeAttribute : MessageAttributeBase
    {
        public ContentTypeAttribute(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            this.ContentType = contentType;
        }

        public string ContentType { get; }

        public override void Apply(IMessageBuilder message)
            => message.ContentType(this.ContentType);
    }
}