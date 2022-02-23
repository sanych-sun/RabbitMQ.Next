using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class DeliverFrameHandlerTests
    {
        [Theory]
        [InlineData(MethodId.BasicReturn)]
        [InlineData(MethodId.BasicAck)]
        public void ShouldIgnoreOtherMethodFrames(MethodId method)
        {
            var frameHandler = this.CreateMock();
            var result = frameHandler.HandleMethodFrame(method, ReadOnlySpan<byte>.Empty);
            Assert.False(result);
        }

        [Theory]
        [InlineData(MethodId.BasicReturn)]
        public async Task ShouldIgnoreContentBeforeDeliverMethod(MethodId method)
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

            var result = frameHandler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.True(result);

            result = await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldTryAllHandlersUntilProcessed()
        {
            var handler1 = this.MockHandler(false);
            var handler2 = this.MockHandler(false);
            var handler3 = this.MockHandler(true);
            var handler4 = this.MockHandler(false);

            var frameHandler = this.CreateMock(new[] { handler1, handler2, handler3, handler4 });

            frameHandler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await handler1.Received().TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>());
            await handler2.Received().TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>());
            await handler3.Received().TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>());
            await handler4.DidNotReceive().TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>());
        }

        [Fact]
        public async Task ShouldAck()
        {
            var ack = Substitute.For<IAcknowledgement>();
            var frameHandler = this.CreateMock(ack: ack);

            frameHandler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await ack.Received().AckAsync(Arg.Any<ulong>());
        }

        [Theory]
        [InlineData(UnprocessedMessageMode.Drop)]
        [InlineData(UnprocessedMessageMode.Requeue)]
        public async Task ShouldNackOnUnprocessed(UnprocessedMessageMode unprocessedMessageMode)
        {
            var handler = this.MockHandler(false);
            var ack = Substitute.For<IAcknowledgement>();
            var frameHandler = this.CreateMock(new[] { handler }, ack, onUnprocessedMessage: unprocessedMessageMode);

            frameHandler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await ack.Received().NackAsync(Arg.Any<ulong>(), unprocessedMessageMode == UnprocessedMessageMode.Requeue);
        }

        [Theory]
        [InlineData(UnprocessedMessageMode.Drop)]
        [InlineData(UnprocessedMessageMode.Requeue)]
        public async Task ShouldNackOnPoisonMessage(UnprocessedMessageMode onPoisonMessage)
        {
            var handler = Substitute.For<IDeliveredMessageHandler>();
            handler.TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>()).ThrowsForAnyArgs(new Exception());
            var ack = Substitute.For<IAcknowledgement>();
            var frameHandler = this.CreateMock(new[] { handler }, ack, onPoisonMessage: onPoisonMessage);

            frameHandler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);
            await frameHandler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            await ack.Received().NackAsync(Arg.Any<ulong>(), onPoisonMessage == UnprocessedMessageMode.Requeue);
        }

        private DeliverFrameHandler CreateMock(
            IReadOnlyList<IDeliveredMessageHandler> handlers = null,
            IAcknowledgement ack = null,
            UnprocessedMessageMode onUnprocessedMessage = UnprocessedMessageMode.Drop,
            UnprocessedMessageMode onPoisonMessage = UnprocessedMessageMode.Drop)
        {
            var deliverMethodParser = new DummyParser<DeliverMethod>(); // cannot use mock here, because of Span in interface parameters
            var serializerFactory = Substitute.For<ISerializerFactory>();
            ack ??= Substitute.For<IAcknowledgement>();
            handlers ??= new[] { this.MockHandler() };

            return new DeliverFrameHandler(serializerFactory, ack, deliverMethodParser,
                handlers, onUnprocessedMessage, onPoisonMessage);
        }

        private IDeliveredMessageHandler MockHandler(bool result = true)
        {
            var handler = Substitute.For<IDeliveredMessageHandler>();
            handler.TryHandleAsync(Arg.Any<DeliveredMessage>(), Arg.Any<IContentAccessor>()).Returns(new ValueTask<bool>(result));

            return handler;
        }
    }
}