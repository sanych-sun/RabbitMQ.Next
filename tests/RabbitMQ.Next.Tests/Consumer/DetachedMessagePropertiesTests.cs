using System;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class DetachedMessagePropertiesTests
    {
        [Fact]
        public void FlagsProperty()
        {
            var val = MessageFlags.ContentType | MessageFlags.Priority;
            var src = Substitute.For<IMessageProperties>();
            src.Flags.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Flags;
            src.ClearReceivedCalls();

            result = prop.Flags;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Flags;
        }

        [Fact]
        public void ContentTypeProperty()
        {
            var val = "contentType";
            var src = Substitute.For<IMessageProperties>();
            src.ContentType.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().ContentType;
            src.ClearReceivedCalls();

            result = prop.ContentType;
            Assert.Equal(val, result);

            result = src.DidNotReceive().ContentType;
        }

        [Fact]
        public void ContentEncodingProperty()
        {
            var val = "contentEncoding";
            var src = Substitute.For<IMessageProperties>();
            src.ContentEncoding.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().ContentEncoding;
            src.ClearReceivedCalls();

            result = prop.ContentEncoding;
            Assert.Equal(val, result);

            result = src.DidNotReceive().ContentEncoding;
        }

        [Fact]
        public void HeadersProperty()
        {
            var val = new Dictionary<string, object>();
            var src = Substitute.For<IMessageProperties>();
            src.Headers.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Headers;
            src.ClearReceivedCalls();

            result = prop.Headers;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Headers;
        }

        [Fact]
        public void DeliveryModeProperty()
        {
            var val = DeliveryMode.Persistent;
            var src = Substitute.For<IMessageProperties>();
            src.DeliveryMode.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().DeliveryMode;
            src.ClearReceivedCalls();

            result = prop.DeliveryMode;
            Assert.Equal(val, result);

            result = src.DidNotReceive().DeliveryMode;
        }
        
        [Fact]
        public void PriorityProperty()
        {
            var val = (byte)42;
            var src = Substitute.For<IMessageProperties>();
            src.Priority.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Priority;
            src.ClearReceivedCalls();

            result = prop.Priority;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Priority;
        }
        
        [Fact]
        public void CorrelationIdProperty()
        {
            var val = "CorrelationId";
            var src = Substitute.For<IMessageProperties>();
            src.CorrelationId.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().CorrelationId;
            src.ClearReceivedCalls();

            result = prop.CorrelationId;
            Assert.Equal(val, result);

            result = src.DidNotReceive().CorrelationId;
        }
        
        [Fact]
        public void ReplyToProperty()
        {
            var val = "ReplyTo";
            var src = Substitute.For<IMessageProperties>();
            src.ReplyTo.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().ReplyTo;
            src.ClearReceivedCalls();

            result = prop.ReplyTo;
            Assert.Equal(val, result);

            result = src.DidNotReceive().ReplyTo;
        }
        
        [Fact]
        public void ExpirationProperty()
        {
            var val = "Expiration";
            var src = Substitute.For<IMessageProperties>();
            src.Expiration.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Expiration;
            src.ClearReceivedCalls();

            result = prop.Expiration;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Expiration;
        }
        
        [Fact]
        public void MessageIdProperty()
        {
            var val = "MessageId";
            var src = Substitute.For<IMessageProperties>();
            src.MessageId.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().MessageId;
            src.ClearReceivedCalls();

            result = prop.MessageId;
            Assert.Equal(val, result);

            result = src.DidNotReceive().MessageId;
        }
        
        [Fact]
        public void TimestampProperty()
        {
            var val = DateTimeOffset.Now;
            var src = Substitute.For<IMessageProperties>();
            src.Timestamp.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Timestamp;
            src.ClearReceivedCalls();

            result = prop.Timestamp;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Timestamp;
        }
        
        [Fact]
        public void TypeProperty()
        {
            var val = "Type";
            var src = Substitute.For<IMessageProperties>();
            src.Type.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().Type;
            src.ClearReceivedCalls();

            result = prop.Type;
            Assert.Equal(val, result);

            result = src.DidNotReceive().Type;
        }
        
        [Fact]
        public void UserIdProperty()
        {
            var val = "UserId";
            var src = Substitute.For<IMessageProperties>();
            src.UserId.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().UserId;
            src.ClearReceivedCalls();

            result = prop.UserId;
            Assert.Equal(val, result);

            result = src.DidNotReceive().UserId;
        }
        
        [Fact]
        public void ApplicationIdProperty()
        {
            var val = "ApplicationId";
            var src = Substitute.For<IMessageProperties>();
            src.ApplicationId.Returns(val);
            var prop = new DetachedMessageProperties(src);

            var result = src.Received().ApplicationId;
            src.ClearReceivedCalls();

            result = prop.ApplicationId;
            Assert.Equal(val, result);

            result = src.DidNotReceive().ApplicationId;
        }
    }
}