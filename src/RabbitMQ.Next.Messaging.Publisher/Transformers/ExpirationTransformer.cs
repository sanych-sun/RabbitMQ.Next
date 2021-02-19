using System;
using System.Globalization;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ExpirationTransformer : IMessageTransformer
    {
        private readonly TimeSpan expiration;

        public ExpirationTransformer(TimeSpan expiration)
        {
            this.expiration = expiration;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.Expiration))
            {
                message.SetExpiration(this.expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}