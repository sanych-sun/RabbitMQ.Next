using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ContentTypeAttribute : Attribute
    {
        public ContentTypeAttribute(string contentType)
        {
            this.ContentType = contentType;
        }

        public string ContentType { get; }
    }
}