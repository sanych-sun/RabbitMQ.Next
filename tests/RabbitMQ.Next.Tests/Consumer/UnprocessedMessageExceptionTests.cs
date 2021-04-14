using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
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
            var serializer = Substitute.For<ISerializer>();
            var content = new Content(serializer, ReadOnlySequence<byte>.Empty);

            var exception = new UnprocessedMessageException(message, properties, content);

            Assert.Equal(message, exception.DeliveredMessage);
            Assert.Equal(properties, exception.Properties);
            Assert.Equal(content, exception.Content);
        }
    }
}