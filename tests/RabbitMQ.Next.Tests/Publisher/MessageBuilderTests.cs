using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class MessageBuilderTests
{
    [Fact]
    public void Exchange()
    {
        var messageBuilder = new MessageBuilder("exchange");
        Assert.Equal("exchange", messageBuilder.Exchange);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnNullOrEmptyExchange(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder(exchange));
    }
    
    [Fact]
    public void RoutingKeyDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.RoutingKey);
    }
    
    [Theory]
    [InlineData("routing")]
    public void CanSetRoutingKey(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetRoutingKey(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetRoutingKey(initialValue);
        ((IMessageBuilder)messageBuilder).SetRoutingKey(value);

        Assert.Equal(value, messageBuilder.RoutingKey);
    }

    [Fact]
    public void ResetRoutingKey()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetRoutingKey("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.RoutingKey);
    }
        
    [Fact]
    public void ContentTypeDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.ContentType);
    }
    
    [Theory]
    [InlineData("content")]
    public void CanSetContentType(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentType(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentType(initialValue);
        ((IMessageBuilder)messageBuilder).SetContentType(value);

        Assert.Equal(value, messageBuilder.ContentType);
    }

    [Fact]
    public void ResetContentType()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentType("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.ContentType);
    }
    
    [Fact]
    public void ContentEncodingDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.ContentEncoding);
    }
    
    [Theory]
    [InlineData("encoding")]
    public void CanSetContentEncoding(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentEncoding(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentEncoding(initialValue);
        ((IMessageBuilder)messageBuilder).SetContentEncoding(value);

        Assert.Equal(value, messageBuilder.ContentEncoding);
    }

    [Fact]
    public void ResetContentEncoding()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetContentEncoding("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.ContentEncoding);
    }

    [Fact]
    public void HeadersDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.Headers);
    }
    
    [Theory]
    [InlineData("key", "value")]
    public void CanSetHeader(string key, string value)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetHeader(key, value);

        Assert.Equal(value, messageBuilder.Headers[key]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void CanSetHeaderThrowsOnInvalidKey(string key)
    {
        var messageBuilder = new MessageBuilder("test");

        Assert.Throws<ArgumentNullException>(() => ((IMessageBuilder)messageBuilder).SetHeader(key, "val"));

    }

    [Theory]
    [InlineData("key", "value", "other", "data")]
    public void CanSetMultipleHeader(string key1, string value1, string key2, string value2)
    {
        var messageBuilder = new MessageBuilder("test");

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetHeader("key", initialValue);
        ((IMessageBuilder)messageBuilder).SetHeader("key", value);

        Assert.Equal(value, messageBuilder.Headers["key"]);
    }

    [Fact]
    public void ResetHeader()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetHeader("key", "value");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.Headers);
    }
        
    [Fact]
    public void DeliveryModeDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Equal(DeliveryMode.Persistent, messageBuilder.DeliveryMode);
    }
    
    [Theory]
    [InlineData(DeliveryMode.NonPersistent)]
    [InlineData(DeliveryMode.Persistent)]
    public void CanSetDeliveryMode(DeliveryMode val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetDeliveryMode(val);

        Assert.Equal(val, messageBuilder.DeliveryMode);
    }

    [Theory]
    [InlineData(DeliveryMode.Unset, DeliveryMode.Persistent)]
    [InlineData(DeliveryMode.Unset, DeliveryMode.NonPersistent)]
    [InlineData(DeliveryMode.Persistent, DeliveryMode.NonPersistent)]
    [InlineData(DeliveryMode.Persistent, DeliveryMode.Unset)]
    public void CanOverrideDeliveryMode(DeliveryMode initialValue, DeliveryMode value)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetDeliveryMode(initialValue);
        ((IMessageBuilder)messageBuilder).SetDeliveryMode(value);

        Assert.Equal(value, messageBuilder.DeliveryMode);
    }

    [Fact]
    public void ResetDeliveryMode()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetDeliveryMode(DeliveryMode.NonPersistent);
        messageBuilder.Reset();

        Assert.Equal(DeliveryMode.Persistent, messageBuilder.DeliveryMode);
    }
        
    [Fact]
    public void PriorityDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Equal(0, messageBuilder.Priority);
    }
    
    [Theory]
    [InlineData(5)]
    public void CanSetPriority(byte val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetPriority(val);

        Assert.Equal(val, messageBuilder.Priority);
    }

    [Theory]
    [InlineData((byte)0, (byte)42)]
    [InlineData((byte)15, (byte)42)]
    [InlineData((byte)15, (byte)0)]
    public void CanOverridePriority(byte initialValue, byte value)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetPriority(initialValue);
        ((IMessageBuilder)messageBuilder).SetPriority(value);

        Assert.Equal(value, messageBuilder.Priority);
    }

    [Fact]
    public void ResetPriority()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetPriority(42);
        messageBuilder.Reset();

        Assert.Equal(0, messageBuilder.Priority);
    }
    
    [Fact]
    public void CorrelationIdDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.CorrelationId);
    }
        
    [Theory]
    [InlineData("correlation")]
    public void CanSetCorrelationId(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetCorrelationId(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetCorrelationId(initialValue);
        ((IMessageBuilder)messageBuilder).SetCorrelationId(value);

        Assert.Equal(value, messageBuilder.CorrelationId);
    }

    [Fact]
    public void ResetCorrelationId()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetCorrelationId("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.CorrelationId);
    }
    
    [Fact]
    public void ReplyToDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.ReplyTo);
    }
        
    [Theory]
    [InlineData("replyTo")]
    public void CanSetReplyTo(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetReplyTo(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetReplyTo(initialValue);
        ((IMessageBuilder)messageBuilder).SetReplyTo(value);

        Assert.Equal(value, messageBuilder.ReplyTo);
    }

    [Fact]
    public void ResetReplyTo()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetReplyTo("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.ReplyTo);
    }
    
    [Fact]
    public void ExpirationDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.Expiration);
    }
        
    [Theory]
    [InlineData("expiration")]
    public void CanSetExpiration(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetExpiration(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetExpiration(initialValue);
        ((IMessageBuilder)messageBuilder).SetExpiration(value);

        Assert.Equal(value, messageBuilder.Expiration);
    }

    [Fact]
    public void ResetExpiration()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetExpiration("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.Expiration);
    }
    
    [Fact]
    public void MessageIdDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.MessageId);
    }
        
    [Theory]
    [InlineData("messageId")]
    public void CanSetMessageId(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetMessageId(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetMessageId(initialValue);
        ((IMessageBuilder)messageBuilder).SetMessageId(value);

        Assert.Equal(value, messageBuilder.MessageId);
    }

    [Fact]
    public void ResetMessageId()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetMessageId("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.MessageId);
    }
        
    [Fact]
    public void TimestampDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Equal(DateTimeOffset.MinValue,  messageBuilder.Timestamp);
    }
    
    [Fact]
    public void CanSetTimestamp()
    {
        var ts = DateTimeOffset.UtcNow;
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetTimestamp(ts);

        Assert.Equal(ts, messageBuilder.Timestamp);
    }

    [Theory]
    [MemberData(nameof(OverrideTimestampTestCases))]
    public void CanOverrideTimestamp(DateTimeOffset initialValue, DateTimeOffset value)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetTimestamp(initialValue);
        ((IMessageBuilder)messageBuilder).SetTimestamp(value);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetTimestamp(DateTimeOffset.UtcNow);
        messageBuilder.Reset();

        Assert.Equal(default, messageBuilder.Timestamp);
    }
        
    [Fact]
    public void TypeDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.Type);
    }
    
    [Theory]
    [InlineData("type")]
    public void CanSetType(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetType(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetType(initialValue);
        ((IMessageBuilder)messageBuilder).SetType(value);

        Assert.Equal(value, messageBuilder.Type);
    }

    [Fact]
    public void ResetType()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetType("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.Type);
    }
        
    [Fact]
    public void UserIdDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.UserId);
    }
    
    [Theory]
    [InlineData("userId")]
    public void CanSetUserId(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetUserId(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetUserId(initialValue);
        ((IMessageBuilder)messageBuilder).SetUserId(value);

        Assert.Equal(value, messageBuilder.UserId);
    }

    [Fact]
    public void ResetUserId()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetUserId("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.UserId);
    }
        
    [Fact]
    public void ApplicationIdDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.Null(messageBuilder.ApplicationId);
    }
    
    [Theory]
    [InlineData("appId")]
    public void CanSetApplicationId(string val)
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetApplicationId(val);

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
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetApplicationId(initialValue);
        ((IMessageBuilder)messageBuilder).SetApplicationId(value);

        Assert.Equal(value, messageBuilder.ApplicationId);
    }

    [Fact]
    public void ResetApplicationId()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetApplicationId("val");
        messageBuilder.Reset();

        Assert.Null(messageBuilder.ApplicationId);
    }
    
    [Fact]
    public void MandatoryDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.False(messageBuilder.Mandatory);
    }
    
    [Fact]
    public void CanSetMandatory()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetMandatory();

        Assert.True(messageBuilder.Mandatory);
    }

    [Fact]
    public void ResetMandatory()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetMandatory();
        messageBuilder.Reset();

        Assert.False(messageBuilder.Mandatory);
    }
    
    [Fact]
    public void ImmediateDefaultValue()
    {
        var messageBuilder = new MessageBuilder("test");
        Assert.False(messageBuilder.Immediate);
    }
    
    [Fact]
    public void CanSetImmediate()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetImmediate();

        Assert.True(messageBuilder.Immediate);
    }

    [Fact]
    public void ResetImmediate()
    {
        var messageBuilder = new MessageBuilder("test");

        ((IMessageBuilder)messageBuilder).SetImmediate();
        messageBuilder.Reset();

        Assert.False(messageBuilder.Immediate);
    }
}