using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class PriorityTransformer : IMessageTransformer
    {
        private readonly byte priority;

        public PriorityTransformer(byte priority)
        {
            this.priority = priority;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (header.Properties.Priority == default)
            {
                header.Properties.Priority = this.priority;
            }
        }
    }
}