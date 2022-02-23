using System;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class ReplyToInitializer : IMessageInitializer
    {
        private readonly string queueName;

        public ReplyToInitializer(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            this.queueName = queueName;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.ReplyTo(this.queueName);
    }
}