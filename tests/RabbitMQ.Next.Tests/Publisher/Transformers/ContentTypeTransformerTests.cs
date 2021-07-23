using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ContentTypeTransformerTests
    {
        [Theory]
        [InlineData("application/text", null)]
        [InlineData("application/text", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new ContentTypeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ContentType.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().ContentType = value;
        }

        [Theory]
        [InlineData("application/text", "text")]
        public void ContentTypeTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new ContentTypeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ContentType.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().ContentType = Arg.Any<string>();
        }
    }
}