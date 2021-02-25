using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ExpirationTransformerTests
    {
        [Theory]
        [InlineData(42, null)]
        [InlineData(42, "")]
        public void CanTransform(int value, string builderValue)
        {
            var ts = TimeSpan.FromSeconds(value);
            var transformer = new ExpirationTransformer(ts);
            var message = Substitute.For<IMessageBuilder>();
            message.Expiration.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().SetExpiration(ts.TotalMilliseconds.ToString());
        }

        [Theory]
        [InlineData(42, "12345")]
        public void ExpirationTransformerDoesNotOverride(int value, string builderValue)
        {
            var ts = TimeSpan.FromSeconds(value);
            var transformer = new ExpirationTransformer(ts);
            var message = Substitute.For<IMessageBuilder>();
            message.Expiration.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetExpiration(Arg.Any<string>());
        }
    }
}