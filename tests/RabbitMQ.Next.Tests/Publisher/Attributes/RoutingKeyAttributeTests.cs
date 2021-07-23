using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
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
        [InlineData("routeKey", null)]
        [InlineData("routeKey", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new RoutingKeyAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.RoutingKey.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().RoutingKey = value;
        }

        [Theory]
        [InlineData("routeKey", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new RoutingKeyAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.RoutingKey.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().RoutingKey = Arg.Any<string>();
        }
    }
}