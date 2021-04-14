using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumerBuilder
    {
        IConsumerBuilder UseFormatter(ITypeFormatter formatter);

        IConsumerBuilder UserFormatterSource(IFormatterSource formatterSource);

        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledgement(Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory);

        IConsumerBuilder OnUnhandledMessage(UnprocessedMessageMode mode);

        IConsumerBuilder OnPoisonMessage(UnprocessedMessageMode mode);

        IConsumerBuilder AddMessageHandler(Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>> handler);
    }
}