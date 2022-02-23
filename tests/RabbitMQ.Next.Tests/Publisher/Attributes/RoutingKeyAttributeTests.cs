using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class RoutingKeyAttributeTests
    {
        [Theory]
        [InlineData("routeKey")]
        public void RoutingKeyAttribute(string value)
        {
            var attr = new RoutingKeyAttribute(value);
            Assert.Equal(value, attr.RoutingKey);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new RoutingKeyAttribute(value));
        }

        [Theory]
        [InlineData("routeKey")]
        public void CanTransform(string value)
        {
            var attr = new RoutingKeyAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().RoutingKey(value);
        }
    }
}