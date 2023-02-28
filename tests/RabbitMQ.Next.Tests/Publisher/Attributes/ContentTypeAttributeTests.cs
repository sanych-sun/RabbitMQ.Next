using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class ContentTypeAttributeTests
{
    [Theory]
    [InlineData("application/json")]
    public void ContentTypeAttribute(string value)
    {
        var attr = new ContentTypeAttribute(value);
        Assert.Equal(value, attr.ContentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowsOnInvalidValue(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new ContentTypeAttribute(value));
    }

    [Theory]
    [InlineData("application/json")]
    public void CanTransform(string value)
    {
        var attr = new ContentTypeAttribute(value);
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().SetContentType(value);
    }
}