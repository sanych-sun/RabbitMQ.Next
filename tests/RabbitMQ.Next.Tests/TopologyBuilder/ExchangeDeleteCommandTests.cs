using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeDeleteCommandTests
{
    [Fact]
    public async Task ExecuteSendsMethod()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeDeleteCommand("exchange");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Any<DeleteMethod>());
    }

    [Fact]
    public async Task CanPassExchangeName()
    {
        var exchangeName = "test-exch";
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeDeleteCommand(exchangeName);

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
            m => m.Exchange == exchangeName));
    }

    [Fact]
    public async Task ShouldNotCancelBindingsByDefault()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeDeleteCommand("exchange");

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
            m => m.UnusedOnly == true));
    }

    [Fact]
    public async Task CanCancelBindings()
    {
        var channel = Substitute.For<IChannel>();
        var builder = new ExchangeDeleteCommand("exchange");
        builder.CancelBindings();

        await builder.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
            m => m.UnusedOnly == false));
    }

    [Theory]
    [InlineData(ReplyCode.NotFound, typeof(ArgumentOutOfRangeException))]
    [InlineData(ReplyCode.PreconditionFailed, typeof(ConflictException))]
    [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
    public async Task ExecuteProcessExceptions(ReplyCode replyCode, Type exceptionType)
    {
        var channel = Substitute.For<IChannel>();
        channel.SendAsync<DeleteMethod, DeleteOkMethod>(default)
            .ReturnsForAnyArgs(new ValueTask<DeleteOkMethod>(Task.FromException<DeleteOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.ExchangeDelete))));
        var builder = new ExchangeDeleteCommand("exchange");

        await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
    }
}