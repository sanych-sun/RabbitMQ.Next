using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class ContentEncodingTransformerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new ContentEncodingInitializer(value));
        }

        [Theory]
        [InlineData("utf-8")]
        public void CanTransform(string value)
        {
            var transformer = new ContentEncodingInitializer(value);
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().ContentEncoding(value);
        }
    }
}