using NSubstitute;
using RabbitMQ.Next.TopologyBuilder;
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

            builder.Received().Argument("alternate-exchange", alternateExchange);
        }
    }
}