using System;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer;

public class ConsumerBuilderTests
{
    [Fact]
    public void PrefetchSize()
    {
        var consumerBuilder = new ConsumerBuilder();
        const uint size = 12345;

        ((IConsumerBuilder) consumerBuilder).PrefetchSize(size);

        Assert.Equal(size, consumerBuilder.PrefetchSize);
    }
        
    [Fact]
    public void PrefetchCount()
    {
        var consumerBuilder = new ConsumerBuilder();
        const ushort count = 42;

        ((IConsumerBuilder) consumerBuilder).PrefetchCount(count);

        Assert.Equal(count, consumerBuilder.PrefetchCount);
    }
        
    [Fact]
    public void ConcurrencyLevel()
    {
        var consumerBuilder = new ConsumerBuilder();
        const byte level = 7;

        ((IConsumerBuilder) consumerBuilder).ConcurrencyLevel(level);

        Assert.Equal(level, consumerBuilder.ConcurrencyLevel);
    }
        
    [Fact]
    public void ThrowsOnIncorrectConcurrencyLevel()
    {
        var consumerBuilder = new ConsumerBuilder();
        const byte level = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => ((IConsumerBuilder) consumerBuilder).ConcurrencyLevel(level));
    }

    [Fact]
    public void SetAcknowledger()
    {
        var consumerBuilder = new ConsumerBuilder();
        var ackFactory = Substitute.For<Func<IChannel, IAcknowledgement>>();

        ((IConsumerBuilder) consumerBuilder).SetAcknowledgement(ackFactory);

        Assert.Equal(ackFactory, consumerBuilder.AcknowledgementFactory);
    }

    [Fact]
    public void SetAcknowledgerThrowsOnNull()
    {
        var consumerBuilder = new ConsumerBuilder();

        Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).SetAcknowledgement(null));
    }
    
    [Fact]
    public void OnPoisonMessage()
    {
        var consumerBuilder = new ConsumerBuilder();
        const PoisonMessageMode val = PoisonMessageMode.Drop;

        ((IConsumerBuilder) consumerBuilder).OnPoisonMessage(val);

        Assert.Equal(val, consumerBuilder.OnPoisonMessage);
    }
    
    [Fact]
    public void DefaultBindToQueue()
    {
        const string queueName = "q1";
        var builder = new ConsumerBuilder();

        ((IConsumerBuilder)builder).BindToQueue(queueName);

        Assert.Contains(builder.Queues, x => x.Queue == queueName);
    }

    [Fact]
    public void CanBindToQueue()
    {
        const string queueName = "q1";
        var consumerBuilder = Substitute.For<Action<IQueueConsumerBuilder>>();
        var builder = new ConsumerBuilder();

        ((IConsumerBuilder)builder).BindToQueue(queueName, consumerBuilder);

        consumerBuilder.Received();
        Assert.Contains(builder.Queues, x => x.Queue == queueName);
    }
}
