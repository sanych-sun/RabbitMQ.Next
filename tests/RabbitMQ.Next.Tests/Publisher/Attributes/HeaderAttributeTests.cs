using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
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
        public void CanTransform(string header, string value)
        {
            var attr = new HeaderAttribute(header, value);
            var builder = Substitute.For<IMessageBuilder>();
            var headers = new Dictionary<string, object>();
            builder.Headers.Returns(headers);

            attr.Apply(builder);

            Assert.Equal(value, headers[header]);
        }

        [Theory]
        [InlineData("header", "value", "existing value")]
        public void DoesNotOverrideExistingValue(string header, string value, string builderValue)
        {
            var attr = new HeaderAttribute(header, value);
            var builder = Substitute.For<IMessageBuilder>();
            var headers = new Dictionary<string, object> {[header] = builderValue};
            builder.Headers.Returns(headers);

            attr.Apply(builder);

            Assert.Equal(builderValue, headers[header]);
        }
    }
}