using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class ApplicationIdInitializerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new ApplicationIdInitializer(value));
        }

        [Theory]
        [InlineData("appId")]
        public void CanTransform(string value)
        {
            var transformer = new ApplicationIdInitializer(value);
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().ApplicationId(value);
        }
    }
}