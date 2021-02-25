using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ApplicationIdAttributeTests
    {
        [Theory]
        [InlineData("app")]
        public void ApplicationIdAttribute(string value)
        {
            var attr = new ApplicationIdAttribute(value);
            Assert.Equal(value, attr.ApplicationId);
        }

        [Theory]
        [InlineData("app", null)]
        [InlineData("app", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new ApplicationIdAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ApplicationId.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetApplicationId(value);
        }

        [Theory]
        [InlineData("app", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new ApplicationIdAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ApplicationId.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetApplicationId(Arg.Any<string>());
        }
    }
}