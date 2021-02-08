using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ReplyToTransformer : IMessageTransformer
    {
        private readonly string queueName;

        public ReplyToTransformer(string queueName)
        {
            this.queueName = queueName;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.ReplyTo))
            {
                header.Properties.ReplyTo = this.queueName;
            }
        }
    }
}