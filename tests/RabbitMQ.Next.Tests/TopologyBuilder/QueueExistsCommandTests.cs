using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueExistsCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyQueue(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new QueueExistsCommand(exchange));
    }
    

    [Theory]
    [InlineData("exchange")]
    public async Task ExecuteCommandAsync(string exchange)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new QueueExistsCommand(exchange);
        
        
        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(
            Arg.Is<DeclareMethod>(b => 
                        string.Equals(b.Queue, exchange)
                        && b.AutoDelete == false
                        && b.Passive == true
                        && b.Arguments == null),
            Arg.Any<CancellationToken>());
    }
}