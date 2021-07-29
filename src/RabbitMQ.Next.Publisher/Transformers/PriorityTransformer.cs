using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
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
            message.Priority ??= this.priority;
        }
    }
}