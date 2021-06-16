using System;
using System.Buffers;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class ChannelCloseHandlerTests
    {
        [Fact]
        public async Task IgnoresOtherMethods()
        {
            var channel = Substitute.For<IChannelInternal>();
            var channelCloseHandler = new ChannelCloseHandler(channel);

            var handled = await channelCloseHandler.HandleAsync(new GetEmptyMethod(), null, ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            channel.DidNotReceive().SetCompleted(Arg.Any<Exception>());
        }

        [Fact]
        public async Task ConfirmsClose()
        {
            var channel = Substitute.For<IChannelInternal>();
            var channelCloseHandler = new ChannelCloseHandler(channel);

            await channelCloseHandler.HandleAsync(new CloseMethod(404, "not exists", MethodId.QueueDeclare), null, ReadOnlySequence<byte>.Empty);

            await channel.Received().SendAsync(new CloseOkMethod());
        }

        [Fact]
        public async Task CompletesWithExceptionOnClose()
        {
            var channel = Substitute.For<IChannelInternal>();
            var channelCloseHandler = new ChannelCloseHandler(channel);

            var handled = await channelCloseHandler.HandleAsync(new CloseMethod(404, "not exists", MethodId.QueueDeclare), null, ReadOnlySequence<byte>.Empty);

            Assert.True(handled);
            channel.Received().SetCompleted(Arg.Is<ChannelException>(ex => ex.ErrorCode == 404 && ex.FailedMethodId == MethodId.QueueDeclare && ex.Message == "not exists"));
        }
    }
}