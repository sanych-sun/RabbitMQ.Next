using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class DelegateReturnMiddlewareTests
{
    [Fact]
    public void ThrowsOnEmptyNext()
    {
        var handler = Substitute.For<Action<IReturnedMessage>>();

        Assert.Throws<ArgumentNullException>(() => new DelegateReturnMiddleware(null, handler));
    }
    
    [Fact]
    public void ThrowsOnEmptyHandler()
    {
        var next = Substitute.For<IReturnMiddleware>();

        Assert.Throws<ArgumentNullException>(() => new DelegateReturnMiddleware(next, null));
    }

    [Fact]
    public void ThrowsOnCancellation()
    {
        var handler = Substitute.For<Action<IReturnedMessage>>();
        var next = Substitute.For<IReturnMiddleware>();

        var message = Substitute.For<IReturnedMessage>();
        var middleware = new DelegateReturnMiddleware(next, handler);
        var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAnyAsync<TaskCanceledException>(async () => await middleware.InvokeAsync(message, cancellation.Token));
        handler.DidNotReceive().Invoke(Arg.Any<IReturnedMessage>());
        next.DidNotReceive().InvokeAsync(Arg.Any<IReturnedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldCallHandler()
    {
        var handler = Substitute.For<Action<IReturnedMessage>>();
        var next = Substitute.For<IReturnMiddleware>();

        var message = Substitute.For<IReturnedMessage>();
        var middleware = new DelegateReturnMiddleware(next, handler);

        await middleware.InvokeAsync(message, default);
        
        handler.Received().Invoke(message);
    }
    
    [Fact]
    public async Task ShouldCallNext()
    {
        var handler = Substitute.For<Action<IReturnedMessage>>();
        var next = Substitute.For<IReturnMiddleware>();

        var message = Substitute.For<IReturnedMessage>();
        var middleware = new DelegateReturnMiddleware(next, handler);

        await middleware.InvokeAsync(message, default);
        
        await next.Received().InvokeAsync(message, Arg.Any<CancellationToken>());
    }
}