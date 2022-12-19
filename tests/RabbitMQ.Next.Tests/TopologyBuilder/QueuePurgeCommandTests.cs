using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueuePurgeCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyQueue(string queue)
    {
        Assert.Throws<ArgumentNullException>(() => new QueuePurgeCommand(queue));
    }


    [Theory]
    [InlineData("queue")]
    public async Task ExecuteCommandAsync(string queue)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new QueuePurgeCommand(queue);
        
        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<PurgeMethod, PurgeOkMethod>(
            Arg.Is<PurgeMethod>(b => 
                string.Equals(b.Queue, queue)),
            Arg.Any<CancellationToken>());
    }
}