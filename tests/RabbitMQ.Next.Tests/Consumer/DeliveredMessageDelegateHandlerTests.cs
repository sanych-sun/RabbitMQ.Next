using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer;

public class DeliveredMessageDelegateHandlerTests
{
    [Fact]
    public void CanCallHandler()
    {
        var message = Substitute.For<IDeliveredMessage>();
        var fn = Substitute.For<Func<IDeliveredMessage, Task<bool>>>();
        var handler = new DeliveredMessageDelegateHandler(fn);

        handler.TryHandleAsync(message);

        fn.Received()(Arg.Any<IDeliveredMessage>());
    }

    [Fact]
    public void ThrowsOnNullHandler()
    {
        Assert.Throws<ArgumentNullException>(() => new DeliveredMessageDelegateHandler(null));
    }

    [Fact]
    public void CanDispose()
    {
        var message = Substitute.For<IDeliveredMessage>();
        var fn = Substitute.For<Func<IDeliveredMessage, Task<bool>>>();
        var handler = new DeliveredMessageDelegateHandler(fn);

        handler.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(() => handler.TryHandleAsync(message));
    }

    [Fact]
    public void CanDisposeMultiple()
    {
        var fn = Substitute.For<Func<IDeliveredMessage, Task<bool>>>();
        var handler = new DeliveredMessageDelegateHandler(fn);

        handler.Dispose();

        var record = Record.Exception(() => handler.Dispose());
        Assert.Null(record);
    }
}