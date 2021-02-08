
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class TypeTransformer : IMessageTransformer
    {
        private readonly string type;

        public TypeTransformer(string type)
        {
            this.type = type;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.Type))
            {
                header.Properties.Type = this.type;
            }
        }
    }
}