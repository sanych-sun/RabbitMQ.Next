using System;
using System.Globalization;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
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
                message.Expiration = this.expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}