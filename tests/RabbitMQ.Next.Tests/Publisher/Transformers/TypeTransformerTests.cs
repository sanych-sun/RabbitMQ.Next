using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class TypeTransformerTests
    {
        [Theory]
        [InlineData("type", null)]
        [InlineData("type", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new TypeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Type.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().Type = value;
        }

        [Theory]
        [InlineData("type", "some")]
        public void TypeTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new TypeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Type.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().Type = Arg.Any<string>();
        }
    }
}