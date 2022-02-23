using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Tests.Mocks
{
    public class MessageProperties : IMessageProperties
    {
        private readonly IReadOnlyDictionary<string, object> headers;
        private readonly MessageFlags flags;
        private readonly string contentType;
        private readonly string contentEncoding;
        private readonly DeliveryMode deliveryMode;
        private readonly byte priority;
        private readonly string correlationId;
        private readonly string replyTo;
        private readonly string expiration;
        private readonly string messageId;
        private readonly DateTimeOffset timestamp;
        private readonly string type;
        private readonly string userId;
        private readonly string applicationId;

        public MessageFlags Flags => this.flags;

        public string RoutingKey { get; init; }

        public string ContentType
        {
            get => this.contentType;
            init
            {
                this.flags |= MessageFlags.ContentType;
                this.contentType = value;
            }
        }

        public string ContentEncoding
        {
            get => this.contentEncoding;
            init
            {
                this.flags |= MessageFlags.ContentEncoding;
                this.contentEncoding = value;
            }
        }

        public IReadOnlyDictionary<string, object> Headers
        {
            get => this.headers;
            init
            {
                this.flags |= MessageFlags.Headers;
                this.headers = value;
            }
        }

        public DeliveryMode DeliveryMode
        {
            get => this.deliveryMode;
            init
            {
                this.flags |= MessageFlags.DeliveryMode;
                this.deliveryMode = value;
            }
        }

        public byte Priority
        {
            get => this.priority;
            init
            {
                this.flags |= MessageFlags.Priority;
                this.priority = value;
            }
        }

        public string CorrelationId
        {
            get => this.correlationId;
            init
            {
                this.flags |= MessageFlags.CorrelationId;
                this.correlationId = value;
            }
        }

        public string ReplyTo
        {
            get => this.replyTo;
            init
            {
                this.flags |= MessageFlags.ReplyTo;
                this.replyTo = value;
            }
        }

        public string Expiration
        {
            get => this.expiration;
            init
            {
                this.flags |= MessageFlags.Expiration;
                this.expiration = value;
            }
        }

        public string MessageId
        {
            get => this.messageId;
            init
            {
                this.flags |= MessageFlags.MessageId;
                this.messageId = value;
            }
        }

        public DateTimeOffset Timestamp
        {
            get => this.timestamp;
            init
            {
                this.flags |= MessageFlags.Timestamp;
                this.timestamp = value;
            }
        }

        public string Type
        {
            get => this.type;
            init
            {
                this.flags |= MessageFlags.Type;
                this.type = value;
            }
        }

        public string UserId
        {
            get => this.userId;
            init
            {
                this.flags |= MessageFlags.UserId;
                this.userId = value;
            }
        }

        public string ApplicationId
        {
            get => this.applicationId;
            init
            {
                this.flags |= MessageFlags.ApplicationId;
                this.applicationId = value;
            }
        }
    }
}