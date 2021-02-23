using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Assembly, AllowMultiple = false)]
    public class ContentEncodingAttribute : MessageAttributeBase
    {
        public ContentEncodingAttribute(string contentEncoding)
        {
            this.ContentEncoding = contentEncoding;
        }

        public string ContentEncoding { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ContentEncoding))
            {
                message.SetContentEncoding(this.ContentEncoding);
            }
        }
    }
}