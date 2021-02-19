using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
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
                message.SetContentType(this.contentType);
            }
        }
    }
}