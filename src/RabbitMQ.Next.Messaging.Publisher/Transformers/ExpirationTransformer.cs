using System;
using System.Globalization;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ExpirationTransformer : IMessageTransformer
    {
        private readonly TimeSpan expiration;

        public ExpirationTransformer(TimeSpan expiration)
        {
            this.expiration = expiration;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.Expiration))
            {
                header.Properties.Expiration = this.expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}