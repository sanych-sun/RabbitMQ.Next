using NSubstitute;
using RabbitMQ.Next.TopologyBuilder;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueBuilderExtensionsTests
{
    [Fact]
    public void WithMessageTtl()
    {
        var ttl = 12345;
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithMessageTtl(ttl);

        builder.Received().Argument("x-message-ttl", ttl);
    }

    [Fact]
    public void WithDeadLetterExchange()
    {
        var exchange = "test";
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithDeadLetterExchange(exchange);

        builder.Received().Argument("x-dead-letter-exchange", exchange);
        builder.DidNotReceive().Argument("x-dead-letter-routing-key", Arg.Any<string>());
    }

    [Fact]
    public void WithDeadLetterExchangeRouting()
    {
        var exchange = "test";
        var routeKey = "route-key";
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithDeadLetterExchange(exchange, routeKey);

        builder.Received().Argument("x-dead-letter-exchange", exchange);
        builder.Received().Argument("x-dead-letter-routing-key", routeKey);
    }

    [Fact]
    public void WithMaxLength()
    {
        var maxLength = 42;
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithMaxLength(maxLength);

        builder.Received().Argument("x-max-length", maxLength);
    }

    [Fact]
    public void WithMaxSize()
    {
        var maxSize = 1024;
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithMaxSize(maxSize);

        builder.Received().Argument("x-max-length-bytes", maxSize);
    }

    [Fact]
    public void WithDropOnOverflow()
    {
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithDropOnOverflow();

        builder.Received().Argument("x-overflow", "drop-head");
    }

    [Fact]
    public void WithRejectOnOverflow()
    {
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithRejectOnOverflow();

        builder.Received().Argument("x-overflow", "reject-publish");
    }

    [Fact]
    public void WithMaxPriority()
    {
        byte maxPriority = 42;
        var builder = Substitute.For<IQueueBuilder>();

        builder.WithMaxPriority(maxPriority);

        builder.Received().Argument("x-max-priority", maxPriority);
    }

    [Fact]
    public void AsLazy()
    {
        var builder = Substitute.For<IQueueBuilder>();

        builder.AsLazy();

        builder.Received().Argument("x-queue-mode", "lazy");
    }
}