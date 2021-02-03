using System;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class TimestampTransformer : IMessageTransformer
    {
        public TimestampTransformer()
        {
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (!header.Properties.Timestamp.HasValue)
            {
                header.Properties.Timestamp = DateTimeOffset.UtcNow;
            }
        }
    }
}