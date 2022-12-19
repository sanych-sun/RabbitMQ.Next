using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeBindCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyDestination(string destination)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeBindCommand(destination, "source", null));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptySource(string source)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeBindCommand("destination", source, null));
    }

    [Theory]
    [InlineData("destination", "source", null)]
    [InlineData("dest", "src", "key")]
    [InlineData("dest", "src", "key", "arg1", "vl1")]
    [InlineData("dest", "src", "key", "arg1", "vl1", "arg2", "vl2")]
    public async Task ExecuteCommandAsync(string destination, string source, string routingKey, params string[] arguments)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new ExchangeBindCommand(destination, source, routingKey);
        var args = arguments.ToArgsDictionary();

        if (args != null)
        {
            foreach (var arg in args)
            {
                cmd.Argument(arg.Key, arg.Value);
            }
        }

        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<BindMethod, BindOkMethod>(
            Arg.Is<BindMethod>(b => 
                string.Equals(b.Destination, destination) 
                && string.Equals(b.Source, source)
                && string.Equals(b.RoutingKey, routingKey)
                && Helpers.DictionaryEquals(b.Arguments, args)),
            Arg.Any<CancellationToken>());
    }
}