using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class PriorityTransformer : IMessageTransformer
    {
        private readonly byte priority;

        public PriorityTransformer(byte priority)
        {
            this.priority = priority;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (message.Priority == default)
            {
                message.SetPriority(this.priority);
            }
        }
    }
}