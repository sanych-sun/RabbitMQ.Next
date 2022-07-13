using System;
using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer;

public class PoisonMessageExceptionTests
{
    [Fact]
    public void PoisonMessageException()
    {
        var message = new DeliveredMessage();
        var content = Substitute.For<IContent>();
        var ex = new Exception();

        var exception = new PoisonMessageException(message, content, ex);

        Assert.Equal(message, exception.DeliveredMessage);
        Assert.Equal(content, exception.Content);
        Assert.Equal(ex, exception.InnerException);
    }
}