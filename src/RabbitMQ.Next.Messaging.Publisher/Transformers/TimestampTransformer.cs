using System;
using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class TimestampTransformer : IMessageTransformer
    {
        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (!message.Timestamp.HasValue)
            {
                message.SetTimestamp(DateTimeOffset.UtcNow);
            }
        }
    }
}