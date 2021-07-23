using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class RoutingKeyTransformerTests
    {
        [Theory]
        [InlineData("route", null)]
        [InlineData("route", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new RoutingKeyTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.RoutingKey.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().RoutingKey = value;
        }

        [Theory]
        [InlineData("route", "test")]
        public void RoutingKeyTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new RoutingKeyTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.RoutingKey.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().RoutingKey = Arg.Any<string>();
        }
    }
}