using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class TypeTransformer : IMessageTransformer
    {
        private readonly string type;

        public TypeTransformer(string type)
        {
            this.type = type;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.Type))
            {
                message.SetType(this.type);
            }
        }
    }
}