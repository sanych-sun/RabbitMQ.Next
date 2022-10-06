using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Tests.Mocks;

public class MessageProperties : IMessageProperties
{
    public string RoutingKey { get; init; }

    public string ContentType { get; init; }

    public string ContentEncoding { get; init; }

    public IReadOnlyDictionary<string, object> Headers { get; init; }

    public DeliveryMode DeliveryMode { get; init; }

    public byte Priority { get; init; }

    public string CorrelationId { get; init; }

    public string ReplyTo { get; init; }

    public string Expiration { get; init; }

    public string MessageId { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public string Type { get; init; }

    public string UserId { get; init; }

    public string ApplicationId { get; init; }
}