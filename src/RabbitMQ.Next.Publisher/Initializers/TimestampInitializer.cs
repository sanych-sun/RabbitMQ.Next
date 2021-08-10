using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class TimestampInitializer : IMessageInitializer
    {
        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.Timestamp(DateTimeOffset.UtcNow);
    }
}