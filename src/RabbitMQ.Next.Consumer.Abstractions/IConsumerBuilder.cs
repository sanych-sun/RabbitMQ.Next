using System;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Consumer;

public interface IConsumerBuilder : ISerializationBuilder<IConsumerBuilder>
{
    IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

    IConsumerBuilder PrefetchSize(uint size);

    IConsumerBuilder PrefetchCount(ushort messages);

    IConsumerBuilder ConcurrencyLevel(byte concurrency);

    IConsumerBuilder SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgementFactory);

    IConsumerBuilder OnPoisonMessage(PoisonMessageMode mode);
}