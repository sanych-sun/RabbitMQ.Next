using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class ApplicationIdAttributeTests
{
    [Theory]
    [InlineData("app")]
    public void ApplicationIdAttribute(string value)
    {
        var attr = new ApplicationIdAttribute(value);
        Assert.Equal(value, attr.ApplicationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowsOnInvalidValue(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new ApplicationIdAttribute(value));
    }

    [Theory]
    [InlineData("app")]
    public void CanTransform(string value)
    {
        var attr = new ApplicationIdAttribute(value);
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().ApplicationId(value);
    }
}