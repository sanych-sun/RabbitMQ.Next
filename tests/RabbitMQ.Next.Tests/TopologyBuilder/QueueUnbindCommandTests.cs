using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueUnbindCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyQueue(string queue)
    {
        Assert.Throws<ArgumentNullException>(() => new QueueUnbindCommand(queue, "source", null));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyExchange(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new QueueUnbindCommand("queue", exchange, null));
    }

    [Theory]
    [InlineData("queue", "exchange", null)]
    [InlineData("queue", "exchange", "key")]
    [InlineData("queue", "exchange", "key", "arg1", "vl1")]
    [InlineData("queue", "exchange", "key", "arg1", "vl1", "arg2", "vl2")]
    public async Task ExecuteCommandAsync(string queue, string exchange, string routingKey, params string[] arguments)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new QueueUnbindCommand(queue, exchange, routingKey);
        var args = arguments.ToArgsDictionary();

        if (args != null)
        {
            foreach (var arg in args)
            {
                cmd.Argument(arg.Key, arg.Value);
            }
        }

        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(
            Arg.Is<UnbindMethod>(b => 
                string.Equals(b.Queue, queue) 
                && string.Equals(b.Exchange, exchange)
                && string.Equals(b.RoutingKey, routingKey)
                && Helpers.DictionaryEquals(b.Arguments, args)),
            Arg.Any<CancellationToken>());
    }
}