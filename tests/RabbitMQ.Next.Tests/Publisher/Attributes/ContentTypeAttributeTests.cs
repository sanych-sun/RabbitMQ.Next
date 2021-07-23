using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ContentTypeAttributeTests
    {
        [Theory]
        [InlineData("application/json")]
        public void ContentTypeAttribute(string value)
        {
            var attr = new ContentTypeAttribute(value);
            Assert.Equal(value, attr.ContentType);
        }

        [Theory]
        [InlineData("application/json", null)]
        [InlineData("application/json", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new ContentTypeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ContentType.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().ContentType = value;
        }

        [Theory]
        [InlineData("application/json", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new ContentTypeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ContentType.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().ContentType = Arg.Any<string>();
        }
    }
}