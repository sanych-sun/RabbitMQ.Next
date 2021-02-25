using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class HeaderAttributeTests
    {
        [Theory]
        [InlineData("header", "value")]
        public void HeaderAttribute(string header, string value)
        {
            var attr = new HeaderAttribute(header, value);
            Assert.Equal(header, attr.Name);
            Assert.Equal(value, attr.Value);
        }

        [Theory]
        [InlineData("header", "value")]
        [InlineData("header", "value")]
        public void CanTransform(string header, string value)
        {
            var attr = new HeaderAttribute(header, value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Headers.Returns(new Dictionary<string, object>());

            attr.Apply(builder);

            builder.Received().SetHeader(header, value);
        }

        [Theory]
        [InlineData("header", "value", "existing value")]
        public void DoesNotOverrideExistingValue(string header, string value, string builderValue)
        {
            var attr = new HeaderAttribute(header, value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Headers.Returns(new Dictionary<string, object> { [header] = builderValue });

            attr.Apply(builder);

            builder.DidNotReceive().SetHeader(header, Arg.Any<string>());
        }
    }
}