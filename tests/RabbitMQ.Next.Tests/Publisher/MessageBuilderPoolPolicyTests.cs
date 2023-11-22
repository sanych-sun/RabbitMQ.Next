using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class MessageBuilderPoolPolicyTests
{
    [Fact]
    public void CreateNew()
    {
        var policy = new MessageBuilderPoolPolicy("test");

        var builder = policy.Create();

        Assert.NotNull(builder);
        Assert.Equal("test", builder.Exchange);
    }

    [Fact]
    public void ReturnResetsMessageBuilder()
    {
        var policy = new MessageBuilderPoolPolicy("any");
        var builder = Substitute.For<MessageBuilder>("test");

        policy.Return(builder);

        builder.Received().Reset();
        Assert.Equal("test", builder.Exchange);
    }
}