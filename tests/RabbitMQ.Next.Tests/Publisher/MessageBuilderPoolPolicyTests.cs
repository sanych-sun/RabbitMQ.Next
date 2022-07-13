using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class MessageBuilderPoolPolicyTests
{
    [Fact]
    public void CreateNew()
    {
        var policy = new MessageBuilderPoolPolicy();

        var builder = policy.Create();

        Assert.NotNull(builder);
    }

    [Fact]
    public void ReturnResetsMessageBuilder()
    {
        var policy = new MessageBuilderPoolPolicy();
        var builder = Substitute.For<MessageBuilder>();

        policy.Return(builder);

        builder.Received().Reset();
    }
}