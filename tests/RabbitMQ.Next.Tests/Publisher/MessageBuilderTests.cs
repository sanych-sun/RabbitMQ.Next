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
        [Theory]
        [InlineData("routing")]
        public void CanSetRoutingKey(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).RoutingKey(val);

            Assert.Equal(val, messageBuilder.RoutingKey);
        }

        [Theory]
        [InlineData(null, "routing")]
        [InlineData("", "routing")]
        [InlineData("key", "routing")]
        [InlineData("routing", null)]
        [InlineData("routing", "")]
        public void CanOverrideRoutingKey(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).RoutingKey(initialValue);
            ((IMessageBuilder)messageBuilder).RoutingKey(value);

            Assert.Equal(value, messageBuilder.RoutingKey);
        }

        [Fact]
        public void ResetRoutingKey()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).RoutingKey("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.RoutingKey);
        }
        
        [Theory]
        [InlineData("content")]
        public void CanSetContentType(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentType(val);

            Assert.Equal(val, messageBuilder.ContentType);
        }

        [Theory]
        [InlineData(null, "content")]
        [InlineData("", "content")]
        [InlineData("key", "content")]
        [InlineData("content", null)]
        [InlineData("content", "")]
        public void CanOverrideContentType(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentType(initialValue);
            ((IMessageBuilder)messageBuilder).ContentType(value);

            Assert.Equal(value, messageBuilder.ContentType);
        }

        [Fact]
        public void ResetContentType()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentType("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.ContentType);
        }
        
        [Theory]
        [InlineData("encoding")]
        public void CanSetContentEncoding(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentEncoding(val);

            Assert.Equal(val, messageBuilder.ContentEncoding);
        }

        [Theory]
        [InlineData(null, "encoding")]
        [InlineData("", "encoding")]
        [InlineData("key", "encoding")]
        [InlineData("encoding", null)]
        [InlineData("encoding", "")]
        public void CanOverrideContentEncoding(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentEncoding(initialValue);
            ((IMessageBuilder)messageBuilder).ContentEncoding(value);

            Assert.Equal(value, messageBuilder.ContentEncoding);
        }

        [Fact]
        public void ResetContentEncoding()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ContentEncoding("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.ContentEncoding);
        }

        [Theory]
        [InlineData("key", "value")]
        public void CanSetHeader(string key, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).SetHeader(key, value);

            Assert.Equal(value, messageBuilder.Headers[key]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CanSetHeaderThrowsOnInvalidKey(string key)
        {
            var messageBuilder = new MessageBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IMessageBuilder)messageBuilder).SetHeader(key, "val"));

        }

        [Theory]
        [InlineData("key", "value", "other", "data")]
        public void CanSetMultipleHeader(string key1, string value1, string key2, string value2)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).SetHeader(key1, value1);
            ((IMessageBuilder)messageBuilder).SetHeader(key2, value2);

            Assert.Equal(value1, messageBuilder.Headers[key1]);
            Assert.Equal(value2, messageBuilder.Headers[key2]);
        }

        [Theory]
        [InlineData(null, "value")]
        [InlineData("", "value")]
        [InlineData("key", "value")]
        [InlineData("value", null)]
        [InlineData("value", "")]
        public void CanOverrideHeader(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).SetHeader("key", initialValue);
            ((IMessageBuilder)messageBuilder).SetHeader("key", value);

            Assert.Equal(value, messageBuilder.Headers["key"]);
        }

        [Fact]
        public void ResetHeader()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).SetHeader("key", "value");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.Headers);
        }
        
        [Theory]
        [InlineData(DeliveryMode.Persistent)]
        public void CanSetDeliveryMode(DeliveryMode val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).DeliveryMode(val);

            Assert.Equal(val, messageBuilder.DeliveryMode);
        }

        [Theory]
        [InlineData(DeliveryMode.Unset, DeliveryMode.Persistent)]
        [InlineData(DeliveryMode.Unset, DeliveryMode.NonPersistent)]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.NonPersistent)]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.Unset)]
        public void CanOverrideDeliveryMode(DeliveryMode initialValue, DeliveryMode value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).DeliveryMode(initialValue);
            ((IMessageBuilder)messageBuilder).DeliveryMode(value);

            Assert.Equal(value, messageBuilder.DeliveryMode);
        }

        [Fact]
        public void ResetDeliveryMode()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).DeliveryMode(DeliveryMode.Persistent);
            messageBuilder.Reset();

            Assert.Equal(default, messageBuilder.DeliveryMode);
        }
        
        [Theory]
        [InlineData(5)]
        public void CanSetPriority(byte val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Priority(val);

            Assert.Equal(val, messageBuilder.Priority);
        }

        [Theory]
        [InlineData((byte)0, (byte)42)]
        [InlineData((byte)15, (byte)42)]
        [InlineData((byte)15, (byte)0)]
        public void CanOverridePriority(byte initialValue, byte value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Priority(initialValue);
            ((IMessageBuilder)messageBuilder).Priority(value);

            Assert.Equal(value, messageBuilder.Priority);
        }

        [Fact]
        public void ResetPriority()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Priority(42);
            messageBuilder.Reset();

            Assert.Equal(0, messageBuilder.Priority);
        }
        
        [Theory]
        [InlineData("correlation")]
        public void CanSetCorrelationId(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).CorrelationId(val);

            Assert.Equal(val, messageBuilder.CorrelationId);
        }

        [Theory]
        [InlineData(null, "correlation")]
        [InlineData("", "correlation")]
        [InlineData("key", "correlation")]
        [InlineData("correlation", null)]
        [InlineData("correlation", "")]
        public void CanOverrideCorrelationId(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).CorrelationId(initialValue);
            ((IMessageBuilder)messageBuilder).CorrelationId(value);

            Assert.Equal(value, messageBuilder.CorrelationId);
        }

        [Fact]
        public void ResetCorrelationId()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).CorrelationId("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.CorrelationId);
        }
        
        [Theory]
        [InlineData("replyTo")]
        public void CanSetReplyTo(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ReplyTo(val);

            Assert.Equal(val, messageBuilder.ReplyTo);
        }

        [Theory]
        [InlineData(null, "replyTo")]
        [InlineData("", "replyTo")]
        [InlineData("key", "replyTo")]
        [InlineData("replyTo", null)]
        [InlineData("replyTo", "")]
        public void CanOverrideReplyTo(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ReplyTo(initialValue);
            ((IMessageBuilder)messageBuilder).ReplyTo(value);

            Assert.Equal(value, messageBuilder.ReplyTo);
        }

        [Fact]
        public void ResetReplyTo()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ReplyTo("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.ReplyTo);
        }
        
        [Theory]
        [InlineData("expiration")]
        public void CanSetExpiration(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Expiration(val);

            Assert.Equal(val, messageBuilder.Expiration);
        }

        [Theory]
        [InlineData(null, "expiration")]
        [InlineData("", "expiration")]
        [InlineData("key", "expiration")]
        [InlineData("expiration", null)]
        [InlineData("expiration", "")]
        public void CanOverrideExpiration(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Expiration(initialValue);
            ((IMessageBuilder)messageBuilder).Expiration(value);

            Assert.Equal(value, messageBuilder.Expiration);
        }

        [Fact]
        public void ResetExpiration()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Expiration("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.Expiration);
        }
        
        [Theory]
        [InlineData("messageId")]
        public void CanSetMessageId(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).MessageId(val);

            Assert.Equal(val, messageBuilder.MessageId);
        }

        [Theory]
        [InlineData(null, "messageId")]
        [InlineData("", "messageId")]
        [InlineData("key", "messageId")]
        [InlineData("messageId", null)]
        [InlineData("messageId", "")]
        public void CanOverrideMessageId(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).MessageId(initialValue);
            ((IMessageBuilder)messageBuilder).MessageId(value);

            Assert.Equal(value, messageBuilder.MessageId);
        }

        [Fact]
        public void ResetMessageId()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).MessageId("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.MessageId);
        }
        
        [Fact]
        public void CanSetTimestamp()
        {
            var ts = DateTimeOffset.UtcNow;
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Timestamp(ts);

            Assert.Equal(ts, messageBuilder.Timestamp);
        }

        [Theory]
        [MemberData(nameof(OverrideTimestampTestCases))]
        public void CanOverrideTimestamp(DateTimeOffset initialValue, DateTimeOffset value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Timestamp(initialValue);
            ((IMessageBuilder)messageBuilder).Timestamp(value);

            Assert.Equal(value, messageBuilder.Timestamp);
        }

        public static IEnumerable<object[]> OverrideTimestampTestCases()
        {
            yield return new object[] { default(DateTimeOffset), DateTimeOffset.UtcNow };
            yield return new object[] { DateTimeOffset.UtcNow, new DateTimeOffset(2021, 08, 10, 00, 31, 00, TimeSpan.Zero) };
            yield return new object[] { DateTimeOffset.UtcNow, default(DateTimeOffset) };
        }

        [Fact]
        public void ResetTimestamp()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Timestamp(DateTimeOffset.UtcNow);
            messageBuilder.Reset();

            Assert.Equal(default, messageBuilder.Timestamp);
        }
        
        [Theory]
        [InlineData("type")]
        public void CanSetType(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Type(val);

            Assert.Equal(val, messageBuilder.Type);
        }

        [Theory]
        [InlineData(null, "type")]
        [InlineData("", "type")]
        [InlineData("key", "type")]
        [InlineData("type", null)]
        [InlineData("type", "")]
        public void CanOverrideType(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Type(initialValue);
            ((IMessageBuilder)messageBuilder).Type(value);

            Assert.Equal(value, messageBuilder.Type);
        }

        [Fact]
        public void ResetType()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).Type("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.Type);
        }
        
        [Theory]
        [InlineData("userId")]
        public void CanSetUserId(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).UserId(val);

            Assert.Equal(val, messageBuilder.UserId);
        }

        [Theory]
        [InlineData(null, "userId")]
        [InlineData("", "userId")]
        [InlineData("key", "userId")]
        [InlineData("userId", null)]
        [InlineData("userId", "")]
        public void CanOverrideUserId(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).UserId(initialValue);
            ((IMessageBuilder)messageBuilder).UserId(value);

            Assert.Equal(value, messageBuilder.UserId);
        }

        [Fact]
        public void ResetUserId()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).UserId("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.UserId);
        }
        
        [Theory]
        [InlineData("appId")]
        public void CanSetApplicationId(string val)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ApplicationId(val);

            Assert.Equal(val, messageBuilder.ApplicationId);
        }

        [Theory]
        [InlineData(null, "appId")]
        [InlineData("", "appId")]
        [InlineData("key", "appId")]
        [InlineData("appId", null)]
        [InlineData("appId", "")]
        public void CanOverrideApplicationId(string initialValue, string value)
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ApplicationId(initialValue);
            ((IMessageBuilder)messageBuilder).ApplicationId(value);

            Assert.Equal(value, messageBuilder.ApplicationId);
        }

        [Fact]
        public void ResetApplicationId()
        {
            var messageBuilder = new MessageBuilder();

            ((IMessageBuilder)messageBuilder).ApplicationId("val");
            messageBuilder.Reset();

            Assert.Null(messageBuilder.ApplicationId);
        }
    }
}