using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            this.routingKey = default;
            this.headers.Clear();
        }

        public MessageFlags Flags => this.flags;

        public string RoutingKey => this.routingKey;

        public string ContentType => this.HasFlag(MessageFlags.ContentType) ? this.contentType : default;

        public string ContentEncoding => this.HasFlag(MessageFlags.ContentEncoding) ? this.contentEncoding : default;

        public IReadOnlyDictionary<string, object> Headers => this.HasFlag(MessageFlags.Headers) ? this.headers : default;

        public DeliveryMode DeliveryMode => this.HasFlag(MessageFlags.DeliveryMode) ? this.deliveryMode : default;

        public byte Priority => this.HasFlag(MessageFlags.Priority) ? this.priority : default;

        public string CorrelationId => this.HasFlag(MessageFlags.CorrelationId) ? this.correlationId : default;

        public string ReplyTo => this.HasFlag(MessageFlags.ReplyTo) ? this.replyTo : default;

        public string Expiration => this.HasFlag(MessageFlags.Expiration) ? this.expiration : default;

        public string MessageId => this.HasFlag(MessageFlags.MessageId) ? this.messageId : default;

        public DateTimeOffset Timestamp => this.HasFlag(MessageFlags.Timestamp) ? this.timestamp : default;

        public string Type => this.HasFlag(MessageFlags.Type) ? this.type : default;

        public string UserId => this.HasFlag(MessageFlags.UserId) ? this.userId : default;

        public string ApplicationId => this.HasFlag(MessageFlags.ApplicationId) ? this.applicationId : default;

        IMessageBuilder IMessageBuilder.RoutingKey(string routingKey)
        {
            this.routingKey = routingKey;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentType(string contentType)
        {
            this.contentType = contentType;
            this.flags |= MessageFlags.ContentType;
            return this;
        }

        IMessageBuilder IMessageBuilder.ContentEncoding(string contentEncoding)
        {
            this.contentEncoding = contentEncoding;
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

        IMessageBuilder IMessageBuilder.DeliveryMode(DeliveryMode deliveryMode)
        {
            this.deliveryMode = deliveryMode;
            this.flags |= MessageFlags.DeliveryMode;
            return this;
        }

        IMessageBuilder IMessageBuilder.Priority(byte priority)
        {
            this.priority = priority;
            this.flags |= MessageFlags.Priority;
            return this;
        }

        IMessageBuilder IMessageBuilder.CorrelationId(string correlationId)
        {
            this.correlationId = correlationId;
            this.flags |= MessageFlags.CorrelationId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ReplyTo(string replyTo)
        {
            this.replyTo = replyTo;
            this.flags |= MessageFlags.ReplyTo;
            return this;
        }

        IMessageBuilder IMessageBuilder.Expiration(string expiration)
        {
            this.expiration = expiration;
            this.flags |= MessageFlags.Expiration;
            return this;
        }

        IMessageBuilder IMessageBuilder.MessageId(string messageId)
        {
            this.messageId = messageId;
            this.flags |= MessageFlags.MessageId;
            return this;
        }

        IMessageBuilder IMessageBuilder.Timestamp(DateTimeOffset timestamp)
        {
            this.timestamp = timestamp;
            this.flags |= MessageFlags.Timestamp;
            return this;
        }

        IMessageBuilder IMessageBuilder.Type(string type)
        {
            this.type = type;
            this.flags |= MessageFlags.Type;
            return this;
        }

        IMessageBuilder IMessageBuilder.UserId(string userId)
        {
            this.userId = userId;
            this.flags |= MessageFlags.UserId;
            return this;
        }

        IMessageBuilder IMessageBuilder.ApplicationId(string applicationId)
        {
            this.applicationId = applicationId;
            this.flags |= MessageFlags.ApplicationId;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasFlag(MessageFlags flag) => (this.flags & flag) == flag;
    }
}