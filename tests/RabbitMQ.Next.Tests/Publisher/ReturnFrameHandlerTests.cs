using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ReturnFrameHandlerTests
    {
        [Theory]
        [InlineData(MethodId.BasicDeliver)]
        [InlineData(MethodId.BasicAck)]
        public async Task ShouldIgnoreOtherMethodFrames(MethodId method)
        {
            var frameHandler = this.CreateMock();
            var result = await frameHandler.HandleMethodFrameAsync(method, ReadOnlyMemory<byte>.Empty);
            Assert.False(result);
        }

        [Theory]
        [InlineData(MethodId.BasicDeliver)]
        public async Task ShouldIgnoreContentBeforeReturnMethod(MethodId method)
        {
            var frameHandler = this.CreateMock();

            var result = await frameHandler.HandleMethodFrameAsync(method, ReadOnlyMemory<byte>.Empty);
            Assert.False(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldIgnoreContentAfterHandled()
        {
            var frameHandler = this.CreateMock();

            var result = await frameHandler.HandleMethodFrameAsync(MethodId.BasicReturn, ReadOnlyMemory<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldHandleReturnMethodFrame()
        {
            var frameHandler = this.CreateMock();

            var result = await frameHandler.HandleMethodFrameAsync(MethodId.BasicReturn, ReadOnlyMemory<byte>.Empty);
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldUseHandlers()
        {
            var handler1 = Substitute.For<IReturnedMessageHandler>();
            handler1.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(false));

            var handler2 = Substitute.For<IReturnedMessageHandler>();
            handler2.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(true));

            var handler3 = Substitute.For<IReturnedMessageHandler>();
            handler3.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(false));

            var frameHandler = this.CreateMock(new[] {handler1, handler2, handler3});

            await frameHandler.HandleMethodFrameAsync(MethodId.BasicReturn, ReadOnlyMemory<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await handler1.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>());
            await handler2.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>());
            await handler3.DidNotReceive().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>());
        }

        private ReturnFrameHandler CreateMock(IReadOnlyList<IReturnedMessageHandler> handlers = null)
        {
            var registry = Substitute.For<IMethodRegistry>();
            var returnMethodParser = Substitute.For<IMethodParser<ReturnMethod>>();
            registry.GetParser<ReturnMethod>().Returns(returnMethodParser);
            var serializer = Substitute.For<ISerializer>();

            return new ReturnFrameHandler(serializer, handlers ?? new [] { Substitute.For<IReturnedMessageHandler>() }, registry);
        }
    }
}