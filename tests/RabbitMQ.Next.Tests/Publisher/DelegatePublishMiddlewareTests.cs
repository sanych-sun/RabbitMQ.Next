using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class DelegatePublishMiddlewareTests
{
    [Fact]
    public void ThrowsOnEmptyNext()
    {
        var handler = Substitute.For<Action<object, IMessageBuilder>>();

        Assert.Throws<ArgumentNullException>(() => new DelegatePublishMiddleware(null, handler));
    }
    
    [Fact]
    public void ThrowsOnEmptyHandler()
    {
        var next = Substitute.For<IPublishMiddleware>();

        Assert.Throws<ArgumentNullException>(() => new DelegatePublishMiddleware(next, null));
    }

    [Fact]
    public void ThrowsOnCancellation()
    {
        var handler = Substitute.For<Action<object, IMessageBuilder>>();
        var next = Substitute.For<IPublishMiddleware>();

        var message = Substitute.For<IMessageBuilder>();
        var middleware = new DelegatePublishMiddleware(next, handler);
        var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAnyAsync<TaskCanceledException>(async () => await middleware.InvokeAsync("test", message, cancellation.Token));
        handler.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<IMessageBuilder>());
        next.DidNotReceive().InvokeAsync(Arg.Any<object>(), Arg.Any<IMessageBuilder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldCallHandler()
    {
        var handler = Substitute.For<Action<object, IMessageBuilder>>();
        var next = Substitute.For<IPublishMiddleware>();

        var message = Substitute.For<IMessageBuilder>();
        var middleware = new DelegatePublishMiddleware(next, handler);

        await middleware.InvokeAsync("test", message, default);
        
        handler.Received().Invoke("test", message);
    }
    
    [Fact]
    public async Task ShouldCallNext()
    {
        var handler = Substitute.For<Action<object, IMessageBuilder>>();
        var next = Substitute.For<IPublishMiddleware>();

        var message = Substitute.For<IMessageBuilder>();
        var middleware = new DelegatePublishMiddleware(next, handler);

        await middleware.InvokeAsync("test", message, default);
        
        await next.Received().InvokeAsync("test", message, Arg.Any<CancellationToken>());
    }
}