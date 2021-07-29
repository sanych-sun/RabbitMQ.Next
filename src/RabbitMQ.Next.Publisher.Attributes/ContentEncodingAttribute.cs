using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Assembly)]
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
                message.ContentEncoding = this.ContentEncoding;
            }
        }
    }
}