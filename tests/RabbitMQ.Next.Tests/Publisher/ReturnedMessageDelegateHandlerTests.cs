using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class ReturnedMessageDelegateHandlerTests
{
    [Fact]
    public void CanCallHandler()
    {
        var message = Substitute.For<IReturnedMessage>();
        var fn = Substitute.For<Func<IReturnedMessage, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.TryHandleAsync(message);

        fn.Received()(Arg.Any<IReturnedMessage>());
    }

    [Fact]
    public void ThrowsOnNullHandler()
    {
        Assert.Throws<ArgumentNullException>(() => new ReturnedMessageDelegateHandler(null));
    }

    [Fact]
    public void CanDispose()
    {
        var message = Substitute.For<IReturnedMessage>();
        var fn = Substitute.For<Func<IReturnedMessage, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(() => handler.TryHandleAsync(message));
    }

    [Fact]
    public void CanDisposeMultiple()
    {
        var fn = Substitute.For<Func<IReturnedMessage, Task<bool>>>();
        var handler = new ReturnedMessageDelegateHandler(fn);

        handler.Dispose();

        var record = Record.Exception(() => handler.Dispose());
        Assert.Null(record);
    }
}