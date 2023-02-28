using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class MandatoryAttributeTests
{
    [Fact]
    public void CanTransform()
    {
        var attr = new MandatoryAttribute();
        var builder = Substitute.For<IMessageBuilder>();

        attr.Apply(builder);

        builder.Received().SetMandatory();
    }
}