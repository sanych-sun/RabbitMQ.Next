using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Assembly, AllowMultiple = false)]
    public class ContentEncodingAttribute : Attribute
    {
        public ContentEncodingAttribute(string contentEncoding)
        {
            this.ContentEncoding = contentEncoding;
        }

        public string ContentEncoding { get; }
    }
}