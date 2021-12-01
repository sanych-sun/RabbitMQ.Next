using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    public class MessageBuilder : IMessageBuilder, IMessageProperties
    {
        private readonly Dictionary<string, object> headers = new();
        private MessageFlags flags;
        private string routingKey;
        private string contentType;
        private string contentEncoding;
        private DeliveryMode deliveryMode;
        private byte priority;
        private string correlationId;
        private string replyTo;
        private string expiration;
        private string messageId;
        private DateTimeOffset timestamp;
        private string type;
        private string userId;
        private string applicationId;

        public void Reset()
        {
            this.flags = MessageFlags.None;
            this.routingKey = null;
            this.contentType = null;
            this.contentEncoding = null;
            this.headers.Clear();
            this.deliveryMode = DeliveryMode.Unset;
            this.priority = default;
            this.correlationId = null;
            this.replyTo = null;
            this.expiration = null;
            this.messageId = null;
            this.timestamp = default;
            this.type = null;
            this.userId = null;
            this.applicationId = null;
        }

        public MessageFlags Flags => this.flags;

        public string RoutingKey => this.routingKey;

        public string ContentType => this.contentType;

        public string ContentEncoding => this.contentEncoding;

        public IReadOnlyDictionary<string, object> Headers => this.headers;

        public DeliveryMode DeliveryMode => this.deliveryMode;

        public byte Priority => this.priority;

        public string CorrelationId => this.correlationId;

        public string ReplyTo => this.replyTo;

        public string Expiration => this.expiration;

        public string MessageId => this.messageId;

        public DateTimeOffset Timestamp => this.timestamp;

        public string Type => this.type;

        public string UserId => this.userId;

        public string ApplicationId => this.applicationId;

        IMessageBuilder IMessageBuilder.RoutingKey(string value)
        {
            this.routingKey = value;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentType(string value)
        {
            this.contentType = value;
            this.flags |= MessageFlags.ContentType;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentEncoding(string value)
        {
            this.contentEncoding = value;
            this.flags |= MessageFlags.ContentEncoding;
            return this;
        }

        public IMessageBuilder SetHeader(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.headers[key] = value;
            this.flags |= MessageFlags.Headers;
            return this;
        }

        IMessageBuilder IMessageBuilder.DeliveryMode(DeliveryMode value)
        {
            this.deliveryMode = value;
            this.flags |= MessageFlags.DeliveryMode;
            return this;
        }

        IMessageBuilder IMessageBuilder.Priority(byte value)
        {
            this.priority = value;
            this.flags |= MessageFlags.Priority;
            return this;
        }

        IMessageBuilder IMessageBuilder.CorrelationId(string value)
        {
            this.correlationId = value;
            this.flags |= MessageFlags.CorrelationId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ReplyTo(string value)
        {
            this.replyTo = value;
            this.flags |= MessageFlags.ReplyTo;
            return this;
        }

        IMessageBuilder IMessageBuilder.Expiration(string value)
        {
            this.expiration = value;
            this.flags |= MessageFlags.Expiration;
            return this;
        }

        IMessageBuilder IMessageBuilder.MessageId(string value)
        {
            this.messageId = value;
            this.flags |= MessageFlags.MessageId;
            return this;
        }

        IMessageBuilder IMessageBuilder.Timestamp(DateTimeOffset value)
        {
            this.timestamp = value;
            this.flags |= MessageFlags.Timestamp;
            return this;
        }

        IMessageBuilder IMessageBuilder.Type(string value)
        {
            this.type = value;
            this.flags |= MessageFlags.Type;
            return this;
        }

        IMessageBuilder IMessageBuilder.UserId(string value)
        {
            this.userId = value;
            this.flags |= MessageFlags.UserId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ApplicationId(string value)
        {
            this.applicationId = value;
            this.flags |= MessageFlags.ApplicationId;
            return this;
        }
    }
}