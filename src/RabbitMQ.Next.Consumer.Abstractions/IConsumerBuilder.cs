using System;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.Consumer
{
    public interface IConsumerBuilder
    {
        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgementFactory);

        IConsumerBuilder OnUnprocessedMessage(UnprocessedMessageMode mode);

        IConsumerBuilder OnPoisonMessage(UnprocessedMessageMode mode);

        IConsumerBuilder MessageHandler(IDeliveredMessageHandler handler);
    }
}