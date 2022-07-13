using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class ContentEncodingAttributeTests
{
    [Theory]
    [InlineData("utf8")]
    public void ContentEncodingAttribute(string value)
    {
        var attr = new ContentEncodingAttribute(value);
        Assert.Equal(value, attr.ContentEncoding);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowsOnInvalidValue(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new ContentEncodingAttribute(value));
    }

    [Theory]
    [InlineData("utf8")]
    public void CanTransform(string value)
    {
        var attr = new ContentEncodingAttribute(value);
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().ContentEncoding(value);
    }

}