using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    public class MessageBuilder : IMessageBuilder, IMessageProperties
    {
        private readonly Dictionary<string, object> headers = new();

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

        public string RoutingKey { get; private set; }
        public string ContentType { get; private set; }
        public string ContentEncoding { get; private set; }
        IReadOnlyDictionary<string, object> IMessageProperties.Headers => this.headers;
        public DeliveryMode DeliveryMode { get; private set; }
        public byte? Priority { get; private set; }
        public string CorrelationId { get; private set; }
        public string ReplyTo { get; private set; }
        public string Expiration { get; private set; }
        public string MessageId { get; private set; }
        public DateTimeOffset? Timestamp { get; private set; }
        public string Type { get; private set; }
        public string UserId { get; private set; }
        public string ApplicationId { get; private set; }

        IMessageBuilder IMessageBuilder.RoutingKey(string routingKey)
        {
            this.RoutingKey = routingKey;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentType(string contentType)
        {
            this.ContentType = contentType;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentEncoding(string contentEncoding)
        {
            this.ContentEncoding = contentEncoding;
            return this;
        }

        public IMessageBuilder SetHeader(string key, object value)
        {
            this.headers[key] = value;
            return this;
        }

        IMessageBuilder IMessageBuilder.DeliveryMode(DeliveryMode deliveryMode)
        {
            this.DeliveryMode = deliveryMode;
            return this;
        }

        IMessageBuilder IMessageBuilder.Priority(byte priority)
        {
            this.Priority = priority;
            return this;
        }

        IMessageBuilder IMessageBuilder.CorrelationId(string correlationId)
        {
            this.CorrelationId = correlationId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ReplyTo(string replyTo)
        {
            this.ReplyTo = replyTo;
            return this;
        }

        IMessageBuilder IMessageBuilder.Expiration(string expiration)
        {
            this.Expiration = expiration;
            return this;
        }

        IMessageBuilder IMessageBuilder.MessageId(string messageId)
        {
            this.MessageId = messageId;
            return this;
        }

        IMessageBuilder IMessageBuilder.Timestamp(DateTimeOffset timestamp)
        {
            this.Timestamp = timestamp;
            return this;
        }

        IMessageBuilder IMessageBuilder.Type(string type)
        {
            this.Type = type;
            return this;
        }

        IMessageBuilder IMessageBuilder.UserId(string userId)
        {
            this.UserId = userId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ApplicationId(string applicationId)
        {
            this.ApplicationId = applicationId;
            return this;
        }
    }
}