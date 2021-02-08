using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ContentTypeTransformer : IMessageTransformer
    {
        private readonly string contentType;

        public ContentTypeTransformer(string contentType)
        {
            this.contentType = contentType;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.ContentType))
            {
                header.Properties.ContentType = this.contentType;
            }
        }
    }
}