using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class MessageBuilderTests
    {
        [Fact]
        public void CtorTest()
        {
            var routingKey = "route";
            var publishFlags = PublishFlags.Immediate;
            var contentType = "contentType";
            var contentEncoding = "contentEncoding";
            var headers = new Dictionary<string, object>();
            var deliveryMode = DeliveryMode.Persistent;
            var priority = (byte)12;
            var correlationId = "correlationId";
            var replyTo = "replyTo";
            var expiration = "expiration";
            var messageId = "messageId";
            var timestamp = DateTimeOffset.Now;
            var type = "type";
            var userId = "userId";
            var applicationId = "applicationId";

            var properties = new MessageProperties
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                Headers = headers,
                DeliveryMode = deliveryMode,
                Priority = priority,
                CorrelationId = correlationId,
                ReplyTo = replyTo,
                Expiration = expiration,
                MessageId = messageId,
                Timestamp = timestamp,
                Type = type,
                UserId = userId,
                ApplicationId = applicationId,
            };

            var builder = new MessageBuilder(routingKey, properties, publishFlags);

            Assert.Equal(routingKey, builder.RoutingKey);
            Assert.Equal(publishFlags, builder.PublishFlags);
            Assert.Equal(contentType, builder.ContentType);
            Assert.Equal(contentEncoding, builder.ContentEncoding);
            Assert.Equal(headers, builder.Headers);
            Assert.Equal(deliveryMode, builder.DeliveryMode);
            Assert.Equal(priority, builder.Priority);
            Assert.Equal(correlationId, builder.CorrelationId);
            Assert.Equal(replyTo, builder.ReplyTo);
            Assert.Equal(expiration, builder.Expiration);
            Assert.Equal(messageId, builder.MessageId);
            Assert.Equal(timestamp, builder.Timestamp);
            Assert.Equal(type, builder.Type);
            Assert.Equal(userId, builder.UserId);
            Assert.Equal(applicationId, builder.ApplicationId);
        }

        [Theory]
        [InlineData("route", null, "")]
        [InlineData("route", "", "")]
        [InlineData("route", "new", "new")]
        public void CanOverrideRoutingKey(string initial, string value, string expected)
        {
            var builder = this.Create(routingKey: initial);

            builder.SetRoutingKey(value);

            Assert.Equal(expected, builder.RoutingKey);
        }

        [Theory]
        [InlineData(PublishFlags.Immediate, PublishFlags.Mandatory, PublishFlags.Mandatory)]
        [InlineData(PublishFlags.Immediate, PublishFlags.None, PublishFlags.None)]
        public void CanOverridePublishFlags(PublishFlags initial, PublishFlags value, PublishFlags expected)
        {
            var builder = this.Create(publishFlags: initial);

            builder.SetPublishFlags(value);

            Assert.Equal(expected, builder.PublishFlags);
        }

        [Theory]
        [InlineData("contentType", null, "")]
        [InlineData("contentType", "", "")]
        [InlineData("contentType", "new", "new")]
        public void CanOverrideContentType(string initial, string value, string expected)
        {
            var builder = this.Create(contentType: initial);

            builder.SetContentType(value);

            Assert.Equal(expected, builder.ContentType);
        }

        [Theory]
        [InlineData("contentEncoding", null, "")]
        [InlineData("contentEncoding", "", "")]
        [InlineData("contentEncoding", "new", "new")]
        public void CanOverrideContentEncoding(string initial, string value, string expected)
        {
            var builder = this.Create(contentEncoding: initial);

            builder.SetContentEncoding(value);

            Assert.Equal(expected, builder.ContentEncoding);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("headerValue", null, null)]
        [InlineData("headerValue", "", "")]
        [InlineData("headerValue", "new", "new")]
        public void CanOverrideHeaders(string initial, string value, string expected)
        {
            var headerKey = "test";
            var headers = new Dictionary<string, object>();
            if (initial != null)
            {
                headers.Add(headerKey, initial);
            }

            var builder = this.Create(headers: headers);

            builder.SetHeader(headerKey, value);

            Assert.Equal(expected, builder.Headers[headerKey]);
        }

        [Theory]
        [InlineData(DeliveryMode.Unset, DeliveryMode.Persistent, DeliveryMode.Persistent)]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.Unset, DeliveryMode.Unset)]
        public void CanOverrideDeliveryMode(DeliveryMode initial, DeliveryMode value, DeliveryMode expected)
        {
            var builder = this.Create(deliveryMode: initial);

            builder.SetDeliveryMode(value);

            Assert.Equal(expected, builder.DeliveryMode);
        }

        [Theory]
        [InlineData(2, 0, 0)]
        [InlineData(2, 5, 5)]
        public void CanOverridePriority(byte initial, byte value, byte expected)
        {
            var builder = this.Create(priority: initial);

            builder.SetPriority(value);

            Assert.Equal(expected, builder.Priority);
        }

        [Theory]
        [InlineData("correlationId", null, "")]
        [InlineData("correlationId", "", "")]
        [InlineData("correlationId", "new", "new")]
        public void CanOverrideCorrelationId(string initial, string value, string expected)
        {
            var builder = this.Create(correlationId: initial);

            builder.SetCorrelationId(value);

            Assert.Equal(expected, builder.CorrelationId);
        }

        [Theory]
        [InlineData("replyTo", null, "")]
        [InlineData("replyTo", "", "")]
        [InlineData("replyTo", "new", "new")]
        public void CanOverrideReplyTo(string initial, string value, string expected)
        {
            var builder = this.Create(replyTo: initial);

            builder.SetReplyTo(value);

            Assert.Equal(expected, builder.ReplyTo);
        }

        [Theory]
        [InlineData("expiration", null, "")]
        [InlineData("expiration", "", "")]
        [InlineData("expiration", "new", "new")]
        public void CanOverrideExpiration(string initial, string value, string expected)
        {
            var builder = this.Create(expiration: initial);

            builder.SetExpiration(value);

            Assert.Equal(expected, builder.Expiration);
        }

        [Theory]
        [InlineData("messageId", null, "")]
        [InlineData("messageId", "", "")]
        [InlineData("messageId", "new", "new")]
        public void CanOverrideMessageId(string initial, string value, string expected)
        {
            var builder = this.Create(messageId: initial);

            builder.SetMessageId(value);

            Assert.Equal(expected, builder.MessageId);
        }

        [Theory]
        [InlineData("type", null, "")]
        [InlineData("type", "", "")]
        [InlineData("type", "new", "new")]
        public void CanOverrideType(string initial, string value, string expected)
        {
            var builder = this.Create(type: initial);

            builder.SetType(value);

            Assert.Equal(expected, builder.Type);
        }

        [Theory]
        [InlineData("userId", null, "")]
        [InlineData("userId", "", "")]
        [InlineData("userId", "new", "new")]
        public void CanOverrideUserId(string initial, string value, string expected)
        {
            var builder = this.Create(userId: initial);

            builder.SetUserId(value);

            Assert.Equal(expected, builder.UserId);
        }

        [Theory]
        [InlineData("applicationId", null, "")]
        [InlineData("applicationId", "", "")]
        [InlineData("applicationId", "new", "new")]
        public void CanOverrideApplicationId(string initial, string value, string expected)
        {
            var builder = this.Create(applicationId: initial);

            builder.SetApplicationId(value);

            Assert.Equal(expected, builder.ApplicationId);
        }

        [Theory]
        [MemberData(nameof(TimestampTestCases))]
        public void CanOverrideTimestamp(DateTimeOffset? initial, DateTimeOffset value)
        {
            var builder = this.Create(timestamp: initial);

            builder.SetTimestamp(value);

            Assert.Equal(value, builder.Timestamp);
        }

        public static IEnumerable<object[]> TimestampTestCases()
        {
            yield return new object[] {null, DateTimeOffset.Now};

            yield return new object[] {DateTimeOffset.Now, DateTimeOffset.UtcNow};
        }

        private MessageBuilder Create(string routingKey = null, PublishFlags publishFlags = PublishFlags.None,
            string contentType = null, string contentEncoding = null, IReadOnlyDictionary<string, object> headers = null,
            DeliveryMode deliveryMode = DeliveryMode.Unset, byte? priority = null, string correlationId = null,
            string replyTo = null, string expiration = null, string messageId = null, DateTimeOffset? timestamp = null,
            string type = null, string userId = null, string applicationId = null)
        {
            var properties = new MessageProperties
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                Headers = headers,
                DeliveryMode = deliveryMode,
                Priority = priority,
                CorrelationId = correlationId,
                ReplyTo = replyTo,
                Expiration = expiration,
                MessageId = messageId,
                Timestamp = timestamp,
                Type = type,
                UserId = userId,
                ApplicationId = applicationId,
            };

            return new MessageBuilder(routingKey, properties, publishFlags);
        }
    }
}