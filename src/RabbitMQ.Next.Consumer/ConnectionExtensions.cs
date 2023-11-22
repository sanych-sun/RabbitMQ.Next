using System;

namespace RabbitMQ.Next.Consumer;

public static class ConnectionExtensions
{
    public static IConsumer Consumer(this IConnection connection, Action<IConsumerBuilder> builder)
    {
        var consumerBuilder = new ConsumerBuilder();
        builder?.Invoke(consumerBuilder);

        if (consumerBuilder.Queues.Count == 0)
        {
            throw new InvalidOperationException("Cannot start consumer without binding to queue. Consider to call BindToQueue.");
        }

        var consumer = new Consumer(connection, consumerBuilder.AcknowledgementFactory,
            consumerBuilder.Queues, consumerBuilder.Middlewares, consumerBuilder.PrefetchSize, consumerBuilder.PrefetchCount,
            consumerBuilder.ConcurrencyLevel, consumerBuilder.OnPoisonMessage);

        return consumer;
    }
}