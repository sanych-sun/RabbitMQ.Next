using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer;

public class QueueConsumerBuilderTests
{
    [Fact]
    public void QueueName()
    {
        const string name = "queue1";

        var builder = new QueueConsumerBuilder(name);

        Assert.Equal(name, builder.Queue);
    }

    [Fact]
    public void CanSetNoLocal()
    {
        var builder = new QueueConsumerBuilder("test");
        ((IQueueConsumerBuilder)builder).NoLocal();

        Assert.True(builder.NoLocal);
    }

    [Fact]
    public void DefaultNoLocal()
    {
        var builder = new QueueConsumerBuilder("test");
        Assert.False(builder.NoLocal);
    }

    [Fact]
    public void CanSetExclusive()
    {
        var builder = new QueueConsumerBuilder("test");
        ((IQueueConsumerBuilder)builder).Exclusive();

        Assert.True(builder.Exclusive);
    }

    [Fact]
    public void DefaultExclusive()
    {
        var builder = new QueueConsumerBuilder("test");
        Assert.False(builder.Exclusive);
    }

    [Fact]
    public void CanSetConsumerTag()
    {
        var builder = new QueueConsumerBuilder("test");
        ((IQueueConsumerBuilder)builder).ConsumerTag("abc");

        Assert.Equal("abc", builder.ConsumerTag);
    }

    [Fact]
    public void DefaultConsumerTag()
    {
        var builder = new QueueConsumerBuilder("test");
        Assert.Null(builder.ConsumerTag);
    }

    [Fact]
    public void CanSetArguments()
    {
        var builder = new QueueConsumerBuilder("test");
        ((IQueueConsumerBuilder)builder).Argument("key", "value");

        Assert.Equal("value", builder.Arguments["key"]);
    }

    [Fact]
    public void DefaultArguments()
    {
        var builder = new QueueConsumerBuilder("test");
        Assert.Null(builder.Arguments);
    }
}
