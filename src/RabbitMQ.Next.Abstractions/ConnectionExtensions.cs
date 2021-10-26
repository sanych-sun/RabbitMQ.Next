using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Abstractions
{
    public static class ConnectionExtensions
    {
        public static Task UseChannelAsync<TState>(this IConnection connection, TState state, Func<TState, IChannel, Task> fn)
            => connection.UseChannelAsync((state, fn), async (st, ch) =>
            {
                await st.fn(st.state, ch);
                return true;
            });

        public async static Task<TResult> UseChannelAsync<TState, TResult>(this IConnection connection, TState state, Func<TState, IChannel, Task<TResult>> fn)
        {
            if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be open.");
            }

            IChannel channel = null;
            try
            {
                channel = await connection.OpenChannelAsync();
                return await fn(state, channel);
            }
            finally
            {
                if (channel != null && !channel.Completion.IsCompleted)
                {
                    await channel.CloseAsync();
                }
            }
        }
    }
}