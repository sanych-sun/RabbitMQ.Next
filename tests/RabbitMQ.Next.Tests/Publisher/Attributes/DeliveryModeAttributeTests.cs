using System;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class DeliveryModeAttributeTests
    {
        [Theory]
        [InlineData(DeliveryMode.Persistent)]
        [InlineData(DeliveryMode.NonPersistent)]
        public void DeliveryModeAttribute(DeliveryMode deliveryMode)
        {
            var attr = new DeliveryModeAttribute(deliveryMode);
            Assert.Equal(deliveryMode, attr.DeliveryMode);
        }

        [Theory]
        [InlineData(DeliveryMode.Unset)]
        public void ThrowsOnInvalidValue(DeliveryMode value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DeliveryModeAttribute(value));
        }

        [Theory]
        [InlineData(DeliveryMode.Persistent)]
        public void CanTransform(DeliveryMode value)
        {
            var attr = new DeliveryModeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().DeliveryMode(value);
        }

    }
}