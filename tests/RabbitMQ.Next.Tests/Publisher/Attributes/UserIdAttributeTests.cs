using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class UserIdAttributeTests
    {
        [Theory]
        [InlineData("myUser")]
        public void UserIdAttribute(string value)
        {
            var attr = new UserIdAttribute(value);
            Assert.Equal(value, attr.UserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ThrowsOnInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new UserIdAttribute(value));
        }

        [Theory]
        [InlineData("myUser")]
        public void CanTransform(string value)
        {
            var attr = new UserIdAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().UserId(value);
        }
    }
}