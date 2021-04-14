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

        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }
        public IReadOnlyDictionary<string, object> Headers { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public byte? Priority { get; set; }
        public string CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public string Expiration { get; set; }
        public string MessageId { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string ApplicationId { get; set; }
    }
}