using System;
using System.Globalization;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ExpirationAttributeTests
    {
        [Theory]
        [InlineData(42)]
        public void ExpirationAttribute(int value)
        {
            var attr = new ExpirationAttribute(value);
            Assert.Equal(TimeSpan.FromSeconds(value), attr.Expiration);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ThrowsOnInvalidValue(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExpirationAttribute(value));
        }

        [Theory]
        [InlineData(42)]
        public void CanTransform(int value)
        {
            var attr = new ExpirationAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().Expiration(TimeSpan.FromSeconds(value).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }
    }
}