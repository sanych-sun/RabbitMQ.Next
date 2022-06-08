using NSubstitute;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Consumer;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class UnprocessedMessageExceptionTests
    {
        [Fact]
        public void UnprocessedMessageException()
        {
            var message = new DeliveredMessage();
            var content = Substitute.For<IContent>();

            var exception = new UnprocessedMessageException(message, content);

            Assert.Equal(message, exception.DeliveredMessage);
            Assert.Equal(content, exception.Content);
        }
    }
}