using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class MessageBuilder : IMessageBuilder
    {
        private readonly MessageProperties baseProperties;
        private string contentType;
        private string contentEncoding;
        private Dictionary<string, object> headers;
        private DeliveryMode? deliveryMode;
        private byte? priority;
        private string correlationId;
        private string replyTo;
        private string expiration;
        private string messageId;
        private DateTimeOffset? timestamp;
        private string type;
        private string userId;
        private string applicationId;

        public MessageBuilder(string routingKey, MessageProperties properties, PublishFlags flags)
        {
            this.RoutingKey = routingKey;
            this.PublishFlags = flags;
            this.baseProperties = properties;
        }

        public string ContentType => this.contentType ?? this.baseProperties.ContentType;

        public string ContentEncoding => this.contentEncoding ?? this.baseProperties.ContentEncoding;

        public IReadOnlyDictionary<string, object> Headers => this.headers ?? this.baseProperties.Headers;

        public DeliveryMode DeliveryMode => this.deliveryMode ?? this.baseProperties.DeliveryMode;

        public byte? Priority => this.priority ?? this.baseProperties.Priority;

        public string CorrelationId => this.correlationId ?? this.baseProperties.CorrelationId;

        public string ReplyTo => this.replyTo ?? this.baseProperties.ReplyTo;

        public string Expiration => this.expiration ?? this.baseProperties.Expiration;

        public string MessageId => this.messageId ?? this.baseProperties.MessageId;

        public DateTimeOffset? Timestamp => this.timestamp ?? this.baseProperties.Timestamp;

        public string Type => this.type ?? this.baseProperties.Type;

        public string UserId => this.userId ?? this.baseProperties.UserId;

        public string ApplicationId => this.applicationId ?? this.baseProperties.ApplicationId;

        public string RoutingKey { get; private set; }

        public PublishFlags PublishFlags { get; private set; }

        public void SetRoutingKey(string value) => this.RoutingKey = value ?? string.Empty;

        public void SetPublishFlags(PublishFlags value) => this.PublishFlags = value;

        public void SetHeader(string key, object value)
        {
            if (this.headers == null)
            {
                var source = this.baseProperties.Headers;

                this.headers = (source == null) ? new Dictionary<string, object>() : new Dictionary<string, object>(source);
            }

            this.headers[key] = value;
        }

        public void SetContentType(string value) => this.contentType = value ?? string.Empty;

        public void SetContentEncoding(string value) => this.contentEncoding = value ?? string.Empty;

        public void SetDeliveryMode(DeliveryMode value) => this.deliveryMode = value;

        public void SetPriority(byte value) => this.priority = value;

        public void SetCorrelationId(string value) => this.correlationId = value ?? string.Empty;

        public void SetReplyTo(string value) => this.replyTo = value ?? string.Empty;

        public void SetExpiration(string value) => this.expiration = value ?? string.Empty;

        public void SetMessageId(string value) => this.messageId = value ?? string.Empty;

        public void SetTimestamp(DateTimeOffset value) => this.timestamp = value;

        public void SetType(string value) => this.type = value ?? string.Empty;

        public void SetUserId(string value) => this.userId = value ?? string.Empty;

        public void SetApplicationId(string value) => this.applicationId = value ?? string.Empty;
    }
}