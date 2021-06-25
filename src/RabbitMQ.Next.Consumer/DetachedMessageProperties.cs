using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer
{
    internal class DetachedMessageProperties : IMessageProperties
    {
        public DetachedMessageProperties(IMessageProperties properties)
        {
            this.ContentType = properties.ContentType;
            this.ContentEncoding = properties.ContentEncoding;
            this.Headers = properties.Headers;
            this.DeliveryMode = properties.DeliveryMode;
            this.Priority = properties.Priority;
            this.CorrelationId = properties.CorrelationId;
            this.ReplyTo = properties.ReplyTo;
            this.Expiration = properties.Expiration;
            this.MessageId = properties.MessageId;
            this.Timestamp = properties.Timestamp;
            this.Type = properties.Type;
            this.UserId = properties.UserId;
            this.ApplicationId = properties.ApplicationId;
        }

        public string ContentType { get; }
        public string ContentEncoding { get; }
        public IReadOnlyDictionary<string, object> Headers { get; }
        public DeliveryMode DeliveryMode { get; }
        public byte? Priority { get; }
        public string CorrelationId { get; }
        public string ReplyTo { get; }
        public string Expiration { get; }
        public string MessageId { get; }
        public DateTimeOffset? Timestamp { get; }
        public string Type { get; }
        public string UserId { get; }
        public string ApplicationId { get; }
    }
}