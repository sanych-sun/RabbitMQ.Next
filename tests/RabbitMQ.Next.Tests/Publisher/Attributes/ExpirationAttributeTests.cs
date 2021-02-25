using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
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
        [InlineData(42, null)]
        [InlineData(42, "")]
        public void CanTransform(int value, string builderValue)
        {
            var attr = new ExpirationAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Expiration.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetExpiration(TimeSpan.FromSeconds(value).TotalMilliseconds.ToString());
        }

        [Theory]
        [InlineData(42, "111")]
        public void DoesNotOverrideExistingValue(int value, string builderValue)
        {
            var attr = new ExpirationAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Expiration.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetExpiration(Arg.Any<string>());
        }
    }
}