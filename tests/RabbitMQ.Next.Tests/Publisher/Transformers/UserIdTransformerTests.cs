using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class UserIdTransformerTests
    {
        [Theory]
        [InlineData("user", null)]
        [InlineData("user", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new UserIdTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.UserId.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().SetUserId(value);
        }

        [Theory]
        [InlineData("user", "Me")]
        public void UserIdTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new UserIdTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.UserId.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetUserId(Arg.Any<string>());
        }
    }
}