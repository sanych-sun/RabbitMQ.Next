using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeBindCommandTests
{
    [Fact]
    public async Task ExecuteSendsMethod()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeBindCommand("dest", "src");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Any<BindMethod>());
    }

    [Fact]
    public async Task CanPassNames()
    {
        var dest = "test-dest";
        var src = "test-src";
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeBindCommand(dest, src);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
            m => m.Destination == dest && m.Source == src));
    }

    [Fact]
    public async Task CanPassRoutingKey()
    {
        var routingKey = "route";
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeBindCommand("dest", "src");
        builder.RoutingKey(routingKey);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
            m => m.RoutingKey == routingKey));
    }

    [Fact]
    public async Task CanPassMultipleRoutingKeys()
    {
        var keys = new List<string>
        {
            "key",
            "other-key"
        };
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeBindCommand("dest", "src");
        foreach (var key in keys)
        {
            builder.RoutingKey(key);
        }

        await builder.ExecuteAsync(channel);

        foreach (var key in keys)
        {
            await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
                m => m.RoutingKey == key));
        }
    }

    [Fact]
    public async Task CanPassArguments()
    {
        var arguments = new Dictionary<string, object>()
        {
            ["test"] = "value",
            ["key2"] = 5,
        };
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeBindCommand("dest", "src");
        foreach (var argument in arguments)
        {
            builder.Argument(argument.Key, argument.Value);
        }

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
            m => arguments.All(a => m.Arguments[a.Key] == a.Value)));
    }

    [Theory]
    [InlineData(ReplyCode.NotFound, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
    public async Task ExecuteProcessExceptions(ReplyCode replyCode, Type exceptionType)
    {
        var channel = Substitute.For<IChannel>();
        channel.SendAsync<BindMethod, BindOkMethod>(default)
            .ReturnsForAnyArgs(new ValueTask<BindOkMethod>(Task.FromException<BindOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.ExchangeBind))));
        var builder = new ExchangeBindCommand("destination", "source");

        await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
    }
}