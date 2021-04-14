using System;
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
        public void CallDelegate()
        {
            var fn = Substitute.For<Func<ReturnedMessage, IMessageProperties, Content, bool>>();
            var handler = new ReturnedMessageDelegateHandler(fn);

            handler.TryHandle(
                new ReturnedMessage("test", "key", 200, "OK"),
                Substitute.For<IMessageProperties>(),
                new Content());

            fn.Received().Invoke(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>());
        }

        [Fact]
        public void DisposedDoesNotCallDelegate()
        {
            var fn = Substitute.For<Func<ReturnedMessage, IMessageProperties, Content, bool>>();
            var handler = new ReturnedMessageDelegateHandler(fn);
            handler.Dispose();

            handler.TryHandle(
                new ReturnedMessage("test", "key", 200, "OK"),
                Substitute.For<IMessageProperties>(),
                new Content());

            fn.DidNotReceive().Invoke(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>());
        }
    }
}