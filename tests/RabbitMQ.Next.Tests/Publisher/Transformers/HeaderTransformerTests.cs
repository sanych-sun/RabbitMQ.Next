using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class HeaderTransformerTests
    {
        [Theory]
        [InlineData("key", "value")]
        public void CanTransform(string header, string value)
        {
            var transformer = new HeaderTransformer(header, value);
            var message = Substitute.For<IMessageBuilder>();
            var headers = new Dictionary<string, object>();
            message.Headers.Returns(headers);

            transformer.Apply(string.Empty, message);

            Assert.Equal(value, headers[header]);
        }

        [Theory]
        [InlineData("key", "value", "123")]
        public void HeaderTransformerDoesNotOverride(string header, string value, string builderValue)
        {
            var transformer = new HeaderTransformer(header, value);
            var message = Substitute.For<IMessageBuilder>();
            var headers = new Dictionary<string, object>() {[header] = builderValue};
            message.Headers.Returns(headers);

            transformer.Apply(string.Empty, message);

            Assert.Equal(builderValue, headers[header]);
        }
    }
}