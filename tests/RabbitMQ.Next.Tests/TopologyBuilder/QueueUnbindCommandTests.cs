using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueUnbindCommandTests
{
    [Fact]
    public async Task ExecuteSendsMethod()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueUnbindCommand("queue", "exchange");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(Arg.Any<UnbindMethod>());
    }

    [Fact]
    public async Task CanPassNames()
    {
        var queueName = "test-queue";
        var exchangeName = "test-exchange";
        var channel = Substitute.For<IChannel>();
        var builder = new QueueUnbindCommand(queueName, exchangeName);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(Arg.Is<UnbindMethod>(
            m => m.Queue == queueName && m.Exchange == exchangeName));
    }

    [Fact]
    public async Task CanPassRoutingKey()
    {
        var routingKey = "route";
        var channel = Substitute.For<IChannel>();
        var builder = new QueueUnbindCommand("queue", "exchange");
        builder.RoutingKey(routingKey);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(Arg.Is<UnbindMethod>(
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
        var builder = new QueueUnbindCommand("queue", "exchange");
        foreach (var key in keys)
        {
            builder.RoutingKey(key);
        }

        await builder.ExecuteAsync(channel);

        foreach (var key in keys)
        {
            await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(Arg.Is<UnbindMethod>(
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
        var builder = new QueueUnbindCommand("queue", "exchange");
        foreach (var argument in arguments)
        {
            builder.Argument(argument.Key, argument.Value);
        }

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<UnbindMethod, UnbindOkMethod>(Arg.Is<UnbindMethod>(
            m => arguments.All(a => m.Arguments[a.Key] == a.Value)));
    }

    [Theory]
    [InlineData(ReplyCode.NotFound, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
    public async Task ExecuteProcessExceptions(ReplyCode replyCode, Type exceptionType)
    {
        var channel = Substitute.For<IChannel>();
        channel.SendAsync<UnbindMethod, UnbindOkMethod>(default)
            .ReturnsForAnyArgs(Task.FromException<UnbindOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.QueueUnbind)));
        var builder = new QueueUnbindCommand("queue", "exchange");

        await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
    }
}