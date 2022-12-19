using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueDeleteCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyQueue(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new QueueDeleteCommand(exchange));
    }
    

    [Theory]
    [InlineData("queue", false, false)]
    [InlineData("queue", true, false)]
    [InlineData("queue", false, true)]
    [InlineData("queue", true, true)]
    public async Task ExecuteCommandAsync(string queue, bool cancelConsumers, bool discardMessages)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new QueueDeleteCommand(queue);
        
        if (cancelConsumers)
        {
            cmd.CancelConsumers();
        }

        if (discardMessages)
        {
            cmd.DiscardMessages();
        }
        
        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(
            Arg.Is<DeleteMethod>(b => 
                string.Equals(b.Queue, queue) 
                && b.UnusedOnly == !cancelConsumers
                && b.EmptyOnly == !discardMessages),
            Arg.Any<CancellationToken>());
    }
}