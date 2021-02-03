using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class UserIdTransformer : IMessageTransformer
    {
        private readonly string userId;

        public UserIdTransformer(string userId)
        {
            this.userId = userId;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.UserId))
            {
                header.Properties.UserId = this.userId;
            }
        }
    }
}