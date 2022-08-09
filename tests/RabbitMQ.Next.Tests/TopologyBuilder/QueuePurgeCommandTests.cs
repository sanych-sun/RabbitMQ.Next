using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueuePurgeCommandTests
{
    [Fact]
    public async Task ExecuteSendsMethod()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new QueuePurgeCommand("queue");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<PurgeMethod, PurgeOkMethod>(Arg.Any<PurgeMethod>());
    }

    [Fact]
    public async Task CanPassExchangeName()
    {
        var queue = "test-queue";
        var channel = Substitute.For<IChannel>();
        var builder = new QueuePurgeCommand(queue);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<PurgeMethod, PurgeOkMethod>(Arg.Is<PurgeMethod>(
            m => m.Queue == queue));
    }

    [Theory]
    [InlineData(ReplyCode.NotFound, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
    public async Task ExecuteProcessExceptions(ReplyCode replyCode, Type exceptionType)
    {
        var channel = Substitute.For<IChannel>();
        channel.SendAsync<PurgeMethod, PurgeOkMethod>(default)
            .ReturnsForAnyArgs(Task.FromException<PurgeOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.ExchangeDelete)));
        var builder = new QueuePurgeCommand("queue");

        await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
    }
}