using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class HeaderInitializerTests
    {
        [Theory]
        [InlineData("", "value")]
        [InlineData(" ", "value")]
        [InlineData(null, "value")]
        public void ThrowsOnInvalidValue(string key, string value)
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderInitializer(key, value));
        }

        [Theory]
        [InlineData("key", "value")]
        public void CanTransform(string key, string value)
        {
            var transformer = new HeaderInitializer(key, value);
            var message = Substitute.For<IMessageBuilder>();
            
            transformer.Apply(string.Empty, message);

            message.Received().SetHeader(key, value);
        }
    }
}