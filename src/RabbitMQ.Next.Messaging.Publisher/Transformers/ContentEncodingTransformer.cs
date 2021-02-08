using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ContentEncodingTransformer : IMessageTransformer
    {
        private readonly string contentEncoding;

        public ContentEncodingTransformer(string contentEncoding)
        {
            this.contentEncoding = contentEncoding;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.ContentEncoding))
            {
                header.Properties.ContentEncoding = this.contentEncoding;
            }
        }
    }
}