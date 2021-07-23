using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class DeliveryModeTransformerTests
    {
        [Theory]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.Unset)]
        public void CanTransform(DeliveryMode value, DeliveryMode builderValue)
        {
            var transformer = new DeliveryModeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.DeliveryMode.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().DeliveryMode = value;
        }

        [Theory]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.NonPersistent)]
        public void DeliveryModeTransformerDoesNotOverride(DeliveryMode value, DeliveryMode builderValue)
        {
            var transformer = new DeliveryModeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.DeliveryMode.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().DeliveryMode = Arg.Any<DeliveryMode>();
        }
    }
}