using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;
using IMessageBuilder = RabbitMQ.Next.Publisher.Abstractions.Transformers.IMessageBuilder;

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

            message.Received().SetDeliveryMode(value);
        }

        [Theory]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.NonPersistent)]
        public void DeliveryModeTransformerDoesNotOverride(DeliveryMode value, DeliveryMode builderValue)
        {
            var transformer = new DeliveryModeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.DeliveryMode.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetDeliveryMode(Arg.Any<DeliveryMode>());
        }
    }
}