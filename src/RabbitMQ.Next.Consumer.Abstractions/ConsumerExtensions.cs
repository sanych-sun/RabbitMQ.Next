using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer;

public static class ConsumerExtensions
{
    public static Task ConsumeAsync(this IConsumer consumer, Action<IDeliveredMessage> handler, CancellationToken cancellation = default)
        => consumer.ConsumeAsync(m =>
        {
            handler(m);
            return default;
        }, cancellation);
}