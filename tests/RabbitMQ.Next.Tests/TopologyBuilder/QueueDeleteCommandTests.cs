using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueDeleteCommandTests
    {
        [Fact]
        public async Task ExecuteSendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand("queue");

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Any<DeleteMethod>());
        }

        [Fact]
        public async Task CanPassExchangeName()
        {
            var queue = "test-queue";
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand(queue);

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
                m => m.Queue == queue));
        }

        [Fact]
        public async Task ShouldNotCancelConsumersByDefault()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand("queue");

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
                m => m.UnusedOnly == true));
        }

        [Fact]
        public async Task ShouldNotDiscardMessagesByDefault()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand("queue");

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
                m => m.EmptyOnly == true));
        }

        [Fact]
        public async Task CanCancelConsumers()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand("queue");
            builder.CancelConsumers();

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
                m => m.UnusedOnly == false));
        }

        [Fact]
        public async Task CanDiscardMessages()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueDeleteCommand("queue");
            builder.DiscardMessages();

            await builder.ExecuteAsync(channel);

            await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(Arg.Is<DeleteMethod>(
                m => m.EmptyOnly == false));
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
            var builder = new QueueDeleteCommand("queue");

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ExecuteAsync(channel));
        }
    }
}