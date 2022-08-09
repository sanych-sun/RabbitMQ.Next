using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class ReturnedMessageDelegateHandlerTests
{
    [Fact]
    public void CanCallHandler()
    {
        var message = new ReturnedMessage();
        var fn = Substitute.For<Func<ReturnedMessage, IContent, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.TryHandleAsync(message, Substitute.For<IContent>());

        fn.Received()(Arg.Any<ReturnedMessage>(), Arg.Any<IContent>());
    }

    [Fact]
    public void ThrowsOnNullHandler()
    {
        Assert.Throws<ArgumentNullException>(() => new ReturnedMessageDelegateHandler(null));
    }

    [Fact]
    public void CanDispose()
    {
        var message = new ReturnedMessage();
        var fn = Substitute.For<Func<ReturnedMessage, IContent, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(() => handler.TryHandleAsync(message, Substitute.For<IContent>()));
    }

    [Fact]
    public void CanDisposeMultiple()
    {
        var fn = Substitute.For<Func<ReturnedMessage, IContent, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.Dispose();

        var record = Record.Exception(() => handler.Dispose());
        Assert.Null(record);
    }
}