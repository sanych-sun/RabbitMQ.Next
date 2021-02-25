using System;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class AttributesTests
    {
        [Theory]
        [InlineData("app")]
        public void ApplicationIdAttribute(string appId)
        {
            var attr = new ApplicationIdAttribute(appId);
            Assert.Equal(appId, attr.ApplicationId);
        }

        [Theory]
        [InlineData("utf8")]
        public void ContentEncodingAttribute(string encoding)
        {
            var attr = new ContentEncodingAttribute(encoding);
            Assert.Equal(encoding, attr.ContentEncoding);
        }
        
        [Theory]
        [InlineData("application/json")]
        public void ContentTypeAttribute(string contentType)
        {
            var attr = new ContentTypeAttribute(contentType);
            Assert.Equal(contentType, attr.ContentType);
        }

        [Theory]
        [InlineData(DeliveryMode.Persistent)]
        [InlineData(DeliveryMode.NonPersistent)]
        [InlineData(DeliveryMode.Unset)]
        public void DeliveryModeAttribute(DeliveryMode deliveryMode)
        {
            var attr = new DeliveryModeAttribute(deliveryMode);
            Assert.Equal(deliveryMode, attr.DeliveryMode);
        }

        [Theory]
        [InlineData("testExchange")]
        public void ExchangeAttribute(string exchange)
        {
            var attr = new ExchangeAttribute(exchange);
            Assert.Equal(exchange, attr.Exchange);
        }
        
        [Theory]
        [InlineData(42)]
        public void ExpirationAttribute(int expirationSeconds)
        {
            var attr = new ExpirationAttribute(expirationSeconds);
            Assert.Equal(TimeSpan.FromSeconds(expirationSeconds), attr.Expiration);
        }

        [Theory]
        [InlineData("header", "value")]
        public void HeaderAttribute(string header, string value)
        {
            var attr = new HeaderAttribute(header, value);
            Assert.Equal(header, attr.Name);
            Assert.Equal(value, attr.Value);
        }

        [Theory]
        [InlineData(10)]
        public void PriorityAttribute(byte priority)
        {
            var attr = new PriorityAttribute(priority);
            Assert.Equal(priority, attr.Priority);
        }

        [Theory]
        [InlineData("replyToQueue")]
        public void ReplyToAttribute(string replyTo)
        {
            var attr = new ReplyToAttribute(replyTo);
            Assert.Equal(replyTo, attr.ReplyTo);
        }

        [Theory]
        [InlineData("routeKey")]
        public void RoutingKeyAttribute(string routingKey)
        {
            var attr = new RoutingKeyAttribute(routingKey);
            Assert.Equal(routingKey, attr.RoutingKey);
        }

        [Theory]
        [InlineData("myType")]
        public void TypeAttribute(string type)
        {
            var attr = new TypeAttribute(type);
            Assert.Equal(type, attr.Type);
        }

        [Theory]
        [InlineData("myUser")]
        public void UserIdAttribute(string userId)
        {
            var attr = new UserIdAttribute(userId);
            Assert.Equal(userId, attr.UserId);
        }
    }
}