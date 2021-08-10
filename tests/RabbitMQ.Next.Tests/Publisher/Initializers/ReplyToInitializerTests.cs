using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class ReplyToTransformerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new ReplyToInitializer(value));
        }

        [Theory]
        [InlineData("queue")]
        public void CanTransform(string value)
        {
            var transformer = new ReplyToInitializer(value);
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().ReplyTo(value);
        }
    }
}