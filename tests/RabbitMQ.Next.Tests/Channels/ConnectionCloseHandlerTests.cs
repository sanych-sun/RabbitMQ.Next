using System;
using System.Buffers;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class ConnectionCloseHandlerTests
    {
        private readonly byte[] CloseMethodPayload = {0x01,0x94,0x0A,0x6E,0x6F,0x74,0x20,0x65,0x78,0x69,0x73,0x74,0x73,0x00,0x32,0x00,0x0A};
        private readonly IMethodRegistry registry = new MethodRegistryBuilder().AddConnectionMethods().Build();

        [Fact]
        public async Task IgnoresOtherMethods()
        {
            var channel = Substitute.For<IChannelInternal>();
            var connectionCloseHandler = new ConnectionCloseHandler(channel, this.registry);

            var handled = await connectionCloseHandler.HandleMethodFrameAsync(MethodId.BasicDeliver, ReadOnlyMemory<byte>.Empty);

            Assert.False(handled);
            channel.DidNotReceive().SetCompleted(Arg.Any<Exception>());
        }

        [Fact]
        public async Task IgnoresContent()
        {
            var channel = Substitute.For<IChannelInternal>();
            var channelCloseHandler = new ConnectionCloseHandler(channel, this.registry);

            var handled = await channelCloseHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            channel.DidNotReceive().SetCompleted(Arg.Any<Exception>());
        }

        [Fact]
        public async Task CompletesWithExceptionOnClose()
        {
            var channel = Substitute.For<IChannelInternal>();
            var connectionCloseHandler = new ConnectionCloseHandler(channel, this.registry);

            var handled = await connectionCloseHandler.HandleMethodFrameAsync(MethodId.ConnectionClose, this.CloseMethodPayload);

            Assert.True(handled);
            channel.Received().SetCompleted(Arg.Is<ConnectionException>(ex => ex.ErrorCode == 404 && ex.Message == "not exists"));
        }
    }
}