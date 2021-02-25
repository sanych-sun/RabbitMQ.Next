using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ExchangeAttributeTests
    {
        [Theory]
        [InlineData("testExchange")]
        public void ExchangeAttribute(string value)
        {
            var attr = new ExchangeAttribute(value);
            Assert.Equal(value, attr.Exchange);
        }

        [Theory]
        [InlineData("testExchange", null)]
        [InlineData("testExchange", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new ExchangeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Exchange.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetExchange(value);
        }

        [Theory]
        [InlineData("testExchange", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new ExchangeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Exchange.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetExchange(Arg.Any<string>());
        }
    }
}