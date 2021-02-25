using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class ExchangeTransformerTests
    {
        [Theory]
        [InlineData("exchange", null)]
        [InlineData("exchange", "")]
        public void CanTransform(string value, string builderValue)
        {
            var transformer = new ExchangeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Exchange.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.Received().SetExchange(value);
        }

        [Theory]
        [InlineData("exchange", "myexchange")]
        public void ExchangeTransformerDoesNotOverride(string value, string builderValue)
        {
            var transformer = new ExchangeTransformer(value);
            var message = Substitute.For<IMessageBuilder>();
            message.Exchange.Returns(builderValue);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetExchange(Arg.Any<string>());
        }
    }
}