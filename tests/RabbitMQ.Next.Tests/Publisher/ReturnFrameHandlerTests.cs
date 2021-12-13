using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ReturnFrameHandlerTests
    {
        [Theory]
        [InlineData(MethodId.BasicDeliver)]
        [InlineData(MethodId.BasicAck)]
        public void ShouldIgnoreOtherMethodFrames(MethodId method)
        {
            var frameHandler = this.CreateMock();
            var result = frameHandler.HandleMethodFrame(method, ReadOnlySpan<byte>.Empty);
            Assert.False(result);
        }

        [Theory]
        [InlineData(MethodId.BasicDeliver)]
        public async Task ShouldIgnoreContentBeforeReturnMethod(MethodId method)
        {
            var frameHandler = this.CreateMock();

            var result = frameHandler.HandleMethodFrame(method, ReadOnlySpan<byte>.Empty);
            Assert.False(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldIgnoreContentAfterHandled()
        {
            var frameHandler = this.CreateMock();

            var result = frameHandler.HandleMethodFrame(MethodId.BasicReturn, ReadOnlySpan<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(result);
        }

        [Fact]
        public void ShouldHandleReturnMethodFrame()
        {
            var frameHandler = this.CreateMock();

            var result = frameHandler.HandleMethodFrame(MethodId.BasicReturn, Array.Empty<byte>());
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldUseHandlers()
        {
            var handler1 = Substitute.For<IReturnedMessageHandler>();
            handler1.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(false));

            var handler2 = Substitute.For<IReturnedMessageHandler>();
            handler2.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(true));

            var handler3 = Substitute.For<IReturnedMessageHandler>();
            handler3.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>())
                .Returns(new ValueTask<bool>(false));

            var frameHandler = this.CreateMock(new[] {handler1, handler2, handler3});

            frameHandler.HandleMethodFrame(MethodId.BasicReturn, ReadOnlySpan<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await handler1.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>());
            await handler2.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>());
            await handler3.DidNotReceive().TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IContentAccessor>());
        }

        private ReturnFrameHandler CreateMock(IReadOnlyList<IReturnedMessageHandler> handlers = null)
        {
            var returnMethodParser = new DummyParser<ReturnMethod>(); // cannot use mock here, because of Span in interface parameters
            var serializerFactory = Substitute.For<ISerializerFactory>();

            return new ReturnFrameHandler(serializerFactory, handlers ?? new [] { Substitute.For<IReturnedMessageHandler>() }, returnMethodParser);
        }
    }
}