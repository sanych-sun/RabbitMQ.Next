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
            var properties = Substitute.For<IMessageProperties>();
            var content = Substitute.For<IContentAccessor>();

            var exception = new UnprocessedMessageException(message, properties, content);

            Assert.Equal(message, exception.DeliveredMessage);
            Assert.Equal(properties, exception.Properties);
            Assert.Equal(content, exception.Content);
        }
    }
}