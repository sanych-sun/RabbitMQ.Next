using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
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