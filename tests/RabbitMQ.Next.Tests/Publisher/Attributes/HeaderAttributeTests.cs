using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class HeaderAttributeTests
    {
        [Theory]
        [InlineData("header", "value")]
        public void HeaderAttribute(string key, string value)
        {
            var attr = new HeaderAttribute(key, value);
            Assert.Equal(key, attr.Name);
            Assert.Equal(value, attr.Value);
        }

        [Theory]
        [InlineData("", "value")]
        [InlineData(" ", "value")]
        [InlineData(null, "value")]
        public void ThrowsOnInvalidValue(string key, string value)
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderAttribute(key, value));
        }

        [Theory]
        [InlineData("header", "value")]
        public void CanTransform(string key, string value)
        {
            var attr = new HeaderAttribute(key, value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().SetHeader(key, value);
        }
    }
}