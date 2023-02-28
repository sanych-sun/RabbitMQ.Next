using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class ReplyToAttributeTests
{
    [Theory]
    [InlineData("replyToQueue")]
    public void ReplyToAttribute(string value)
    {
        var attr = new ReplyToAttribute(value);
        Assert.Equal(value, attr.ReplyTo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowsOnInvalidValue(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new ReplyToAttribute(value));
    }
        
    [Theory]
    [InlineData("replyToQueue")]
    public void CanTransform(string value)
    {
        var attr = new ReplyToAttribute(value);
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().SetReplyTo(value);
    }
}