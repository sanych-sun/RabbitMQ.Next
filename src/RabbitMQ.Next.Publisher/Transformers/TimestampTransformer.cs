using System;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class TimestampTransformer : IMessageTransformer
    {
        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            message.Timestamp ??= DateTimeOffset.UtcNow;
        }
    }
}