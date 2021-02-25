using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ReplyToTransformerTests
    {
        [Theory]
        [InlineData("queue", null)]
        [InlineData("queue", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new ReplyToTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ReplyTo.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().SetReplyTo(value);
        }

        [Theory]
        [InlineData("queue", "other")]
        public void ReplyToTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new ReplyToTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ReplyTo.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetReplyTo(Arg.Any<string>());
        }
    }
}