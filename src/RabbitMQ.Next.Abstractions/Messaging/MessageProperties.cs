using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Abstractions.Messaging
{
    public class MessageProperties
    {
        public string ContentType { get; set; }

        public string ContentEncoding { get; set; }

        public Dictionary<string, object> Headers { get; set; }

        public DeliveryMode DeliveryMode { get; set; }

        public byte Priority { get; set; }

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