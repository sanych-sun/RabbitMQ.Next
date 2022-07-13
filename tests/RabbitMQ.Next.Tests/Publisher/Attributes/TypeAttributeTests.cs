using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class TypeAttributeTests
{
    [Theory]
    [InlineData("myType")]
    public void TypeAttribute(string value)
    {
        var attr = new TypeAttribute(value);
        Assert.Equal(value, attr.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowsOnInvalidValue(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new TypeAttribute(value));
    }

    [Theory]
    [InlineData("myType")]
    public void CanTransform(string value)
    {
        var attr = new TypeAttribute(value);
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().Type(value);
    }
}