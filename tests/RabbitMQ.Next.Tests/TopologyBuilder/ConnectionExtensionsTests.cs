using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.TopologyBuilder;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ConnectionExtensionsTests
    {
        [Fact]
        public void TopologyBuilderCreate()
        {
            var connection = Substitute.For<IConnection>();

            var topologyBuilder = connection.TopologyBuilder();

            Assert.Equal(connection, topologyBuilder.Connection);
        }
    }
}