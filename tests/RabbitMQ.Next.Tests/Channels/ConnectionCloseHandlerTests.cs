using System;
using System.Buffers;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class ConnectionCloseHandlerTests
    {
        [Fact]
        public async Task IgnoresOtherMethods()
        {
            var channel = Substitute.For<IChannelInternal>();
            var connectionCloseHandler = new ConnectionCloseHandler(channel);

            var handled = await connectionCloseHandler.HandleAsync(new GetEmptyMethod(), null, ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            channel.DidNotReceive().SetCompleted(Arg.Any<Exception>());
        }

        [Fact]
        public async Task CompletesWithExceptionOnClose()
        {
            var channel = Substitute.For<IChannelInternal>();
            var connectionCloseHandler = new ConnectionCloseHandler(channel);

            var handled = await connectionCloseHandler.HandleAsync(new CloseMethod(404, "not exists", MethodId.QueueDeclare), null, ReadOnlySequence<byte>.Empty);

            Assert.True(handled);
            channel.Received().SetCompleted(Arg.Is<ConnectionException>(ex => ex.ErrorCode == 404 && ex.Message == "not exists"));
        }
    }
}