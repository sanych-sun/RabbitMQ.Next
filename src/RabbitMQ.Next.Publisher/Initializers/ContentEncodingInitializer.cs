using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class ContentEncodingInitializer : IMessageInitializer
    {
        private readonly string contentEncoding;

        public ContentEncodingInitializer(string contentEncoding)
        {
            if (string.IsNullOrWhiteSpace(contentEncoding))
            {
                throw new ArgumentNullException(nameof(contentEncoding));
            }

            this.contentEncoding = contentEncoding;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.ContentEncoding(this.contentEncoding);
    }
}