using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next;

public static class ConnectionExtensions
{
    public static Task UseChannelAsync<TState>(this IConnection connection, TState state, Func<TState, IChannel, Task> fn)
        => connection.UseChannelAsync((state, fn), async (st, ch) =>
        {
            await st.fn(st.state, ch).ConfigureAwait(false);
            return true;
        });

    public static async Task<TResult> UseChannelAsync<TState, TResult>(this IConnection connection, TState state, Func<TState, IChannel, Task<TResult>> fn)
    {
        IChannel channel = null;
        try
        {
            channel = await connection.OpenChannelAsync().ConfigureAwait(false);
            return await fn(state, channel).ConfigureAwait(false);
        }
        finally
        {
            if (channel != null && !channel.Completion.IsCompleted)
            {
                await channel.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}