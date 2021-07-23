using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ContentEncodingAttributeTests
    {
        [Theory]
        [InlineData("utf8")]
        public void ContentEncodingAttribute(string value)
        {
            var attr = new ContentEncodingAttribute(value);
            Assert.Equal(value, attr.ContentEncoding);
        }

        [Theory]
        [InlineData("utf8", null)]
        [InlineData("utf8", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new ContentEncodingAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ContentEncoding.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().ContentEncoding = value;
        }

        [Theory]
        [InlineData("utf8", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new ContentEncodingAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ContentEncoding.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().ContentEncoding = Arg.Any<string>();
        }
    }
}