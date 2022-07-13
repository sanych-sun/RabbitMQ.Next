using System;
using System.Globalization;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers;

public class ExpirationInitializerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ThrowsOnInvalidValue(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpirationInitializer(TimeSpan.FromSeconds(value)));
    }

    [Theory]
    [InlineData(42)]
    public void CanTransform(int value)
    {
        var ts = TimeSpan.FromSeconds(value);
        var transformer = new ExpirationInitializer(ts);
        var message = Substitute.For<IMessageBuilder>();

        transformer.Apply(string.Empty, message);

        message.Received().Expiration(ts.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

}