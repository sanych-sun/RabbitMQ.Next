using System;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers;

public class DeliveryModeInitializerTests
{
    [Theory]
    [InlineData(DeliveryMode.Unset)]
    public void ThrowsOnInvalidValue(DeliveryMode value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DeliveryModeInitializer(value));
    }

    [Theory]
    [InlineData(DeliveryMode.Persistent)]
    [InlineData(DeliveryMode.NonPersistent)]
    public void CanTransform(DeliveryMode value)
    {
        var transformer = new DeliveryModeInitializer(value);
        var message = Substitute.For<IMessageBuilder>();

        transformer.Apply(string.Empty, message);

        message.Received().DeliveryMode(value);
    }

}