using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class PublisherBuilderTests
{
    [Fact]
    public void CanRegisterMiddleware()
    {
        var middleware1 = Substitute.For<Func<IPublishMiddleware, IPublishMiddleware>>();
        var middleware2 = Substitute.For<Func<IPublishMiddleware, IPublishMiddleware>>();

        var builder = new PublisherBuilder();
        ((IPublisherBuilder) builder).UsePublishMiddleware(middleware1);
        ((IPublisherBuilder) builder).UsePublishMiddleware(middleware2);
        
        Assert.Equal(new[] { middleware1, middleware2}, builder.PublishMiddlewares);
    }

    [Fact]
    public void ThrowsOnInvalidMiddleware()
    {
        var builder = new PublisherBuilder();
            
        Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UsePublishMiddleware(null));
    }
        
    [Fact]
    public void CanRegisterReturnedMessageHandler()
    {
        var middleware = Substitute.For<Func<IReturnMiddleware,IReturnMiddleware>>();

        var builder = new PublisherBuilder();
        ((IPublisherBuilder) builder).UseReturnMiddleware(middleware);

        Assert.Contains(middleware, builder.ReturnMiddlewares);
    }

    [Fact]
    public void ThrowsOnInvalidReturnedMessageHandler()
    {
        var builder = new PublisherBuilder();
            
        Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseReturnMiddleware(null));
    }
}