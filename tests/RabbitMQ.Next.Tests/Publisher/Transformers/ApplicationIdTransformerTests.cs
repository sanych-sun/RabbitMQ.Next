using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ApplicationIdTransformerTests
    {
        [Theory]
        [InlineData("appId", null)]
        [InlineData("appId", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new ApplicationIdTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ApplicationId.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().ApplicationId = value;
        }

        [Theory]
        [InlineData("appId", "text")]
        public void DoesNotOverride(string value, string builderValue)
        {
            var transformer = new ApplicationIdTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.ApplicationId.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().ApplicationId = Arg.Any<string>();
        }
    }
}