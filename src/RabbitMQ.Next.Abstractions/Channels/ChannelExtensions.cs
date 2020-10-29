using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public static class ChannelExtensions
    {
        public static Task<TResponse> SendAsync<TRequest, TResponse>(this IChannel channel, TRequest request)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod
        {
            return channel.UseSyncChannel(async (ch, r) =>
            {
                var waitTask = ch.WaitAsync<TResponse>();
                await ch.SendAsync(r);
                return await waitTask;
            }, request);
        }
    }
}