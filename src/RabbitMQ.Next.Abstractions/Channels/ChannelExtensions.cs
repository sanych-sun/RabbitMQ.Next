using System;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public static class ChannelExtensions
    {
        public static void OnCompleted(this IChannel channel, Action<Exception> handler)
        {
            channel.Completion.ContinueWith(t => handler(t.Exception?.InnerException ?? t.Exception));
        }
    }
}