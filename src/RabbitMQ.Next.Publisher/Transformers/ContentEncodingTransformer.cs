using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
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
                message.ContentEncoding = this.contentEncoding;
            }
        }
    }
}