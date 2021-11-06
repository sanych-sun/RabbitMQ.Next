using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class PriorityTransformerTests
    {
        [Theory]
        [InlineData(5)]
        public void CanTransform(byte value)
        {
            var transformer = new PriorityInitializer(value);
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().Priority(value);
        }
    }
}