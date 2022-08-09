using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueDeclareCommandTests
{
    [Fact]
    public async Task ExecuteSendsMethod()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Any<DeclareMethod>());
    }

    [Fact]
    public async Task CanPassQueueName()
    {
        var queueName = "test-queue";
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand(queueName);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => m.Queue == queueName));
    }

    [Fact]
    public async Task DurableByDefault()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => m.Durable));
    }

    [Fact]
    public async Task CanMakeTransient()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");
        builder.Transient();

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => !m.Durable));
    }

    [Fact]
    public async Task NoAutoDeleteByDefault()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => !m.AutoDelete));
    }

    [Fact]
    public async Task CanMakeAutoDelete()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");
        builder.AutoDelete();

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => m.AutoDelete));
    }

    [Fact]
    public async Task NoExclusiveByDefault()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => !m.Exclusive));
    }

    [Fact]
    public async Task CanMakeExclusive()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueueDeclareCommand("queue");
        builder.Exclusive();

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => m.Exclusive));
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
        var builder = new QueueDeclareCommand("queue");

        foreach (var arg in arguments)
        {
            builder.Argument(arg.Key, arg.Value);
        }

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
            m => arguments.All(a => m.Arguments[a.Key] == a.Value)));
    }

    [Theory]
    [InlineData(ReplyCode.AccessRefused, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.PreconditionFailed, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.ResourceLocked, typeof(ConflictException))]
    [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
    public async Task ExecuteProcessExceptions(ReplyCode replyCode, Type exceptionType)
    {
        var channel = Substitute.For<IChannel>();
        channel.SendAsync<DeclareMethod, DeclareOkMethod>(default)
            .ReturnsForAnyArgs(Task.FromException<DeclareOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.QueueBind)));
        var builder = new QueueDeclareCommand("queue");

        await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
    }
}