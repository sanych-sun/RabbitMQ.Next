using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    public class MessageBuilder : IMessageBuilder, IMessageProperties
    {
        private readonly Dictionary<string, object> headers = new();

        public string RoutingKey { get; set; }
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }
        public IDictionary<string, object> Headers => this.headers;

        IReadOnlyDictionary<string, object> IMessageProperties.Headers => this.headers;

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

        public void Reset()
        {
            this.RoutingKey = null;
            this.ContentType = null;
            this.ContentEncoding = null;
            this.headers.Clear();
            this.DeliveryMode = DeliveryMode.Unset;
            this.Priority = null;
            this.CorrelationId = null;
            this.ReplyTo = null;
            this.Expiration = null;
            this.MessageId = null;
            this.Timestamp = null;
            this.Type = null;
            this.UserId = null;
            this.ApplicationId = null;
        }
    }
}