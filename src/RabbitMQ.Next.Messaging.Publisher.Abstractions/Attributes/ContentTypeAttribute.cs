using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ContentTypeAttribute : MessageAttributeBase
    {
        public ContentTypeAttribute(string contentType)
        {
            this.ContentType = contentType;
        }

        public string ContentType { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ContentType))
            {
                message.SetContentType(this.ContentType);
            }
        }
    }
}