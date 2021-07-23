using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class ContentTypeTransformer : IMessageTransformer
    {
        private readonly string contentType;

        public ContentTypeTransformer(string contentType)
        {
            this.contentType = contentType;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ContentType))
            {
                message.ContentType = this.contentType;
            }
        }
    }
}