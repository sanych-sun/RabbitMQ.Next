using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
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
            message.Headers.Returns(new Dictionary<string, object>());

            transformer.Apply(string.Empty, message);

            message.Received().SetHeader(header, value);
        }

        [Theory]
        [InlineData("key", "value")]
        public void HeaderTransformerDoesNotOverride(string header, string value)
        {
            var transformer = new HeaderTransformer(header, value);
            var message = Substitute.For<IMessageBuilder>();
            message.Headers.Returns(new Dictionary<string, object>() {["key"] = "123"});

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetHeader(Arg.Any<string>(), Arg.Any<object>());
        }
    }
}