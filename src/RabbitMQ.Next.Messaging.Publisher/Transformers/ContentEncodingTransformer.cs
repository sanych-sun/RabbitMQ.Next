using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ContentEncodingTransformer : IMessageTransformer
    {
        private readonly string contentEncoding;

        public ContentEncodingTransformer(string contentEncoding)
        {
            this.contentEncoding = contentEncoding;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ContentEncoding))
            {
                message.SetContentEncoding(this.contentEncoding);
            }
        }
    }
}