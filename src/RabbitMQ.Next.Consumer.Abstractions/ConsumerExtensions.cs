using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public static class ConsumerExtensions
{
    public static Task ConsumeAsync(this IConsumer consumer, Action<IDeliveredMessage,IContentAccessor> handler, CancellationToken cancellation = default)
        => consumer.ConsumeAsync((m, c) =>
        {
            handler(m, c);
            return default;
        }, cancellation);
}