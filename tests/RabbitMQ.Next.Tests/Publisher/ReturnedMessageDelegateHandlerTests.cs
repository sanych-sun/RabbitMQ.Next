using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ReturnedMessageDelegateHandlerTests
    {
        [Fact]
        public void ThrowsOnNullHandler()
        {
            Assert.Throws<ArgumentNullException>(() => new ReturnedMessageDelegateHandler(null));
        }

        [Fact]
        public async Task CallDelegate()
        {
            var fn = Substitute.For<Func<ReturnedMessage, IMessageProperties, IContentAccessor, ValueTask<bool>>>();
            var handler = new ReturnedMessageDelegateHandler(fn);

            await handler.TryHandleAsync(
                new ReturnedMessage("test", "key", 200, "OK"),
                Substitute.For<IMessageProperties>(),
                Substitute.For<IContentAccessor>());

            await fn.Received().Invoke(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>());
        }

        [Fact]
        public async Task DisposedDoesNotCallDelegate()
        {
            var fn = Substitute.For<Func<ReturnedMessage, IMessageProperties, IContentAccessor, ValueTask<bool>>>();
            var handler = new ReturnedMessageDelegateHandler(fn);
            handler.Dispose();

            await handler.TryHandleAsync(
                new ReturnedMessage("test", "key", 200, "OK"),
                Substitute.For<IMessageProperties>(),
                Substitute.For<IContentAccessor>());

            await fn.DidNotReceive().Invoke(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<IContentAccessor>());
        }
    }
}