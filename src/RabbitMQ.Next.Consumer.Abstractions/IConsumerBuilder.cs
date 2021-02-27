using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumerBuilder
    {
        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledgementMode(AcknowledgementMode mode);

        IConsumerBuilder SetUnhandledMessageMode(UnhandledMessageMode mode);

        IConsumerBuilder AddMessageHandler(Func<IDeliveredMessage, IContent, ValueTask<bool>> handler);
    }
}