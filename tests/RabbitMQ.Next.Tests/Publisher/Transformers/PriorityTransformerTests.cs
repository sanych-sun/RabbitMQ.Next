using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class PriorityTransformerTests
    {
        [Theory]
        [InlineData(5, null)]
        public void CanTransform(byte value, byte? builderValue)
        {
            var transformer = new PriorityTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Priority.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().SetPriority(value);
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, 2)]
        public void PriorityTransformerDoesNotOverride(byte value, byte builderValue)
        {
            var transformer = new PriorityTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Priority.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetPriority(Arg.Any<byte>());
        }
    }
}