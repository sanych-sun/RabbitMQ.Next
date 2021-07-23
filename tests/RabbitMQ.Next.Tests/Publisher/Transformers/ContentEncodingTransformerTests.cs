using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ContentEncodingTransformerTests
    {
        [Theory]
        [InlineData("utf-8", null)]
        [InlineData("utf-8", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new ContentEncodingTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ContentEncoding.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().ContentEncoding = value;
        }

        [Theory]
        [InlineData("utf-8", "plain")]
        public void DoesNotOverride(string value, string builderValue)
        {
            var transformer = new ContentEncodingTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ContentEncoding.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().ContentEncoding = Arg.Any<string>();
        }
    }
}