using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IMessageBuilder
    {
        string RoutingKey { get; set; }
        string ContentType { get; set; }
        string ContentEncoding { get; set; }
        IDictionary<string, object> Headers { get; }
        DeliveryMode DeliveryMode { get; set; }
        byte? Priority { get; set; }
        string CorrelationId { get; set; }
        string ReplyTo { get; set; }
        string Expiration { get; set; }
        string MessageId { get; set; }
        DateTimeOffset? Timestamp { get; set; }
        string Type { get; set; }
        string UserId { get; set; }
        string ApplicationId { get; set; }
    }
}