using System;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumerBuilder : ISerializationBuilder<IConsumerBuilder>
    {
        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgementFactory);

        IConsumerBuilder OnUnprocessedMessage(UnprocessedMessageMode mode);

        IConsumerBuilder OnPoisonMessage(UnprocessedMessageMode mode);

        IConsumerBuilder AddMessageHandler(IDeliveredMessageHandler handler);
    }
}