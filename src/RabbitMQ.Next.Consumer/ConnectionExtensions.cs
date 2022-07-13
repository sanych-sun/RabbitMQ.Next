using System;

namespace RabbitMQ.Next.Consumer;

public static class ConnectionExtensions
{
    public static IConsumer Consumer(this IConnection connection, Action<IConsumerBuilder> builder)
    {
        var consumerBuilder = new ConsumerBuilder();
        builder?.Invoke(consumerBuilder);

        var consumer = new Consumer(connection, consumerBuilder.AcknowledgementFactory,
            consumerBuilder.Handlers, consumerBuilder.Queues, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount,
            consumerBuilder.ConcurrencyLevel, consumerBuilder.OnUnprocessedMessage, consumerBuilder.OnPoisonMessage);

        return consumer;
    }
}