using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class PriorityAttributeTests
    {
        [Theory]
        [InlineData(10)]
        public void PriorityAttribute(byte value)
        {
            var attr = new PriorityAttribute(value);
            Assert.Equal(value, attr.Priority);
        }

        [Theory]
        [InlineData(42)]
        public void CanTransform(byte value)
        {
            var attr = new PriorityAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();

            attr.Apply(builder);

            builder.Received().Priority(value);
        }
    }
}