﻿using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumerBuilder
    {
        IConsumerBuilder UseFormatter(ITypeFormatter formatter);

        IConsumerBuilder UseFormatterSource(IFormatterSource formatterSource);

        IConsumerBuilder BindToQueue(string queue, Action<IQueueConsumerBuilder> builder = null);

        IConsumerBuilder PrefetchSize(uint size);

        IConsumerBuilder PrefetchCount(ushort messages);

        IConsumerBuilder SetAcknowledger(Func<IAcknowledgement, IAcknowledger> acknowledgerFactory);

        IConsumerBuilder OnUnprocessedMessage(UnprocessedMessageMode mode);

        IConsumerBuilder OnPoisonMessage(UnprocessedMessageMode mode);

        IConsumerBuilder AddMessageHandler(Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>> handler);
    }
}