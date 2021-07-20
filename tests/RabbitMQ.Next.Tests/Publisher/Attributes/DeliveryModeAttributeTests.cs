using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;
using IMessageBuilder = RabbitMQ.Next.Publisher.Abstractions.Transformers.IMessageBuilder;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class DeliveryModeAttributeTests
    {
        [Theory]
        [InlineData(DeliveryMode.Persistent)]
        [InlineData(DeliveryMode.NonPersistent)]
        [InlineData(DeliveryMode.Unset)]
        public void DeliveryModeAttribute(DeliveryMode deliveryMode)
        {
            var attr = new DeliveryModeAttribute(deliveryMode);
            Assert.Equal(deliveryMode, attr.DeliveryMode);
        }

        [Theory]
        [InlineData(DeliveryMode.Persistent, DeliveryMode.Unset)]
        [InlineData(DeliveryMode.NonPersistent, DeliveryMode.Unset)]
        public void CanTransform(DeliveryMode value, DeliveryMode builderValue)
        {
            var attr = new DeliveryModeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.DeliveryMode.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetDeliveryMode(value);
        }

        [Theory]
        [InlineData(DeliveryMode.NonPersistent, DeliveryMode.Persistent)]
        public void DoesNotOverrideExistingValue(DeliveryMode value, DeliveryMode builderValue)
        {
            var attr = new DeliveryModeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.DeliveryMode.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetDeliveryMode(Arg.Any<DeliveryMode>());
        }
    }
}