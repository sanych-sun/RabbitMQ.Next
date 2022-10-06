using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Messaging;

public interface IMessageProperties
{
    string ContentType { get; }

    string ContentEncoding { get; }

    IReadOnlyDictionary<string, object> Headers { get; }

    DeliveryMode DeliveryMode { get; }

    byte Priority { get; }

    string CorrelationId { get; }

    string ReplyTo { get; }

    string Expiration { get; }

    string MessageId { get; }

    DateTimeOffset Timestamp { get; }

    string Type { get; }

    string UserId { get; }

    string ApplicationId { get; }
}