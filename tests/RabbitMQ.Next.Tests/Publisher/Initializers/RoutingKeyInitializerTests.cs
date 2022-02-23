using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class RoutingKeyInitializerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new RoutingKeyInitializer(value));
        }

        [Theory]
        [InlineData("route")]
        public void CanTransform(string value)
        {
            var transformer = new RoutingKeyInitializer(value);
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().RoutingKey(value);
        }
    }
}