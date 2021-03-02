using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumerBuilder
    {
        IConsumerBuilder UseFormatter(IFormatter formatter);

        IConsumerBuilder UserFormatterSource(IFormatterSource formatters);

        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledgement(Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory);

        IConsumerBuilder OnUnhandledMessage(UnhandledMessageMode mode);

        IConsumerBuilder OnPoisonMessage(UnhandledMessageMode mode);

        IConsumerBuilder AddMessageHandler(Func<IDeliveredMessage, IContent, ValueTask<bool>> handler);
    }
}