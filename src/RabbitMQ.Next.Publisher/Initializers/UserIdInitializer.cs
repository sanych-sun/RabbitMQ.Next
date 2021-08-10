using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class UserIdInitializer : IMessageInitializer
    {
        private readonly string userId;

        public UserIdInitializer(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            this.userId = userId;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.UserId(this.userId);
    }
}