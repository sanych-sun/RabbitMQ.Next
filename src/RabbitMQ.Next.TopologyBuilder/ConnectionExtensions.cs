using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.TopologyBuilder;

public static class ConnectionExtensions
{
    public static async Task ConfigureAsync(this IConnection connection, Func<ITopologyBuilder, Task> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        using var topologyBuilder = new TopologyBuilder(connection);
        await builder.Invoke(topologyBuilder).ConfigureAwait(false);
    }
    
    public static async Task ConfigureAsync<TState>(this IConnection connection, TState state, Func<ITopologyBuilder, TState, Task> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        using var topologyBuilder = new TopologyBuilder(connection);
        await builder.Invoke(topologyBuilder, state).ConfigureAwait(false);
    }
}