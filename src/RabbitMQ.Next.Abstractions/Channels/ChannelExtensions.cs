using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public static class ChannelExtensions
    {
        public static Task<TResponse> SendAsync<TRequest, TResponse>(this IChannel channel, TRequest request, CancellationToken cancellationToken = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod
        {
            return channel.UseSyncChannel(async (ch, state) =>
            {
                var waitTask = ch.WaitAsync<TResponse>(state.cancellationToken);
                await ch.SendAsync(state.request);
                return await waitTask;
            }, (request, cancellationToken));
        }
    }
}