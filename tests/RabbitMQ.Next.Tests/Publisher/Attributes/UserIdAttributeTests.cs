using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class UserIdAttributeTests
    {
        [Theory]
        [InlineData("myUser")]
        public void UserIdAttribute(string value)
        {
            var attr = new UserIdAttribute(value);
            Assert.Equal(value, attr.UserId);
        }

        [Theory]
        [InlineData("myUser", null)]
        [InlineData("myUser", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new UserIdAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.UserId.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetUserId(value);
        }

        [Theory]
        [InlineData("myUser", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new UserIdAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.UserId.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetUserId(Arg.Any<string>());
        }
    }
}