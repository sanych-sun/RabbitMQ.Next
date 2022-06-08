using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Messaging;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class DeliveredMessageDelegateHandlerTests
    {
        [Fact]
        public void CanCallHandler()
        {
            var fn = Substitute.For<Func<DeliveredMessage, IContent, ValueTask<bool>>>();
            var handler = new DeliveredMessageDelegateHandler(fn);

            handler.TryHandleAsync(new DeliveredMessage(), Substitute.For<IContent>());

            fn.Received()(Arg.Any<DeliveredMessage>(), Arg.Any<IContent>());
        }

        [Fact]
        public void ThrowsOnNullHandler()
        {
            Assert.Throws<ArgumentNullException>(() => new DeliveredMessageDelegateHandler(null));
        }

        [Fact]
        public void CanDispose()
        {
            var fn = Substitute.For<Func<DeliveredMessage, IContent, ValueTask<bool>>>();
            var handler = new DeliveredMessageDelegateHandler(fn);

            handler.Dispose();

            Assert.Throws<ObjectDisposedException>(() => handler.TryHandleAsync(new DeliveredMessage(), Substitute.For<IContent>()));
        }

        [Fact]
        public void CanDisposeMultiple()
        {
            var fn = Substitute.For<Func<DeliveredMessage, IContent, ValueTask<bool>>>();
            var handler = new DeliveredMessageDelegateHandler(fn);

            handler.Dispose();

            var record = Record.Exception(() => handler.Dispose());
            Assert.Null(record);
        }
    }
}