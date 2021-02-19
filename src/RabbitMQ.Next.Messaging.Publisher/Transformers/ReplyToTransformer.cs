using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ReplyToTransformer : IMessageTransformer
    {
        private readonly string queueName;

        public ReplyToTransformer(string queueName)
        {
            this.queueName = queueName;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ReplyTo))
            {
                message.SetReplyTo(this.queueName);
            }
        }
    }
}