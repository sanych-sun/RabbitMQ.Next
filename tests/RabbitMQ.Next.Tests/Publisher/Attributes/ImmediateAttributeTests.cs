using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class ImmediateAttributeTests
{
    [Fact]
    public void CanTransform()
    {
        var attr = new ImmediateAttribute();
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().SetImmediate();
    }
}