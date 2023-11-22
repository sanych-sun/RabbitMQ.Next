using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IConsumerBuilder
{
    IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

    IConsumerBuilder PrefetchSize(uint size);

    IConsumerBuilder PrefetchCount(ushort messages);

    IConsumerBuilder ConcurrencyLevel(byte concurrency);

    IConsumerBuilder SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgementFactory);

    IConsumerBuilder OnPoisonMessage(PoisonMessageMode mode);
    
    IConsumerBuilder UseConsumerMiddleware(Func<IDeliveredMessage,IContentAccessor,Func<IDeliveredMessage,IContentAccessor,Task>,Task> middleware);
}