using System;

namespace RabbitMQ.Next.Messaging
{
    [Flags]
    public enum MessageFlags : ushort
    {
        None = 0,
        ApplicationId = 1 << 3,
        UserId = 1 << 4,
        Type = 1 << 5,
        Timestamp = 1 << 6,
        MessageId = 1 << 7,
        Expiration = 1 << 8,
        ReplyTo = 1 << 9,
        CorrelationId = 1 << 10,
        Priority = 1 << 11,
        DeliveryMode = 1 << 12,
        Headers = 1 << 13,
        ContentEncoding = 1 << 14,
        ContentType = 1 << 15,
    }
}