using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;

namespace RabbitMQ.Next.TopologyBuilder
{
    public static class ConnectionExtensions
    {
        public static ITopologyBuilder TopologyBuilder(this IConnection connection)
            => new TopologyBuilder(connection);
    }
}