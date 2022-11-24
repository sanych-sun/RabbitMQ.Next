using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class PublisherBuilderTests
{
    [Fact]
    public void CanRegisterTransformers()
    {
        var transformer1 = Substitute.For<IMessageInitializer>();
        var transformer2 = Substitute.For<IMessageInitializer>();

        var builder = new PublisherBuilder();
        ((IPublisherBuilder) builder).UseMessageInitializer(transformer1);
        ((IPublisherBuilder) builder).UseMessageInitializer(transformer2);

        Assert.Contains(transformer1, builder.Initializers);
        Assert.Contains(transformer2, builder.Initializers);
    }

    [Fact]
    public void ThrowsOnInvalidTransformer()
    {
        var builder = new PublisherBuilder();
            
        Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseMessageInitializer(null));
    }
        
    [Fact]
    public void CanRegisterReturnedMessageHandler()
    {
        var handler = Substitute.For<Func<IReturnedMessage,Task>>();

        var builder = new PublisherBuilder();
        ((IPublisherBuilder) builder).OnReturnedMessage(handler);

        Assert.Equal(handler, builder.ReturnedMessageHandler);
    }

    [Fact]
    public void ThrowsOnInvalidReturnedMessageHandler()
    {
        var builder = new PublisherBuilder();
            
        Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).OnReturnedMessage(null));
    }

    [Fact]
    public void ConfirmsDefault()
    {
        var builder = new PublisherBuilder();

        Assert.True(builder.PublisherConfirms);
    }

    [Fact]
    public void NoConfirms()
    {
        var builder = new PublisherBuilder();
        ((IPublisherBuilder)builder).NoConfirm();

        Assert.False(builder.PublisherConfirms);
    }
}