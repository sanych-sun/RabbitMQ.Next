using NSubstitute;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ExchangeBuilderExtensionsTests
    {
        [Fact]
        public void WithAlternateExchange()
        {
            var alternateExchange = "testAE";
            var builder = Substitute.For<IExchangeBuilder>();

            builder.WithAlternateExchange(alternateExchange);

            builder.Received().SetArgument("alternate-exchange", alternateExchange);
        }
    }
}