using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class UserIdTransformer : IMessageTransformer
    {
        private readonly string userId;

        public UserIdTransformer(string userId)
        {
            this.userId = userId;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.UserId))
            {
                message.SetUserId(this.userId);
            }
        }
    }
}