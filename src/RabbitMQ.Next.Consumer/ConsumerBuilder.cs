using System;
using System.Collections.Generic;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.Consumer;

internal class ConsumerBuilder : IConsumerBuilder
{
    private readonly List<QueueConsumerBuilder> queues = new();
    private readonly List<IDeliveredMessageHandler> handlers = new();

    public ConsumerBuilder()
    {
        this.AcknowledgementFactory = ch => new DefaultAcknowledgement(ch);
        this.PrefetchCount = 10;
        this.ConcurrencyLevel = 1;
    }
        
    public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

    public IReadOnlyList<IDeliveredMessageHandler> Handlers => this.handlers;

    public uint PrefetchSize { get; private set; }

    public ushort PrefetchCount { get; private set; }
        
    public byte ConcurrencyLevel { get; private set; }

    public Func<IChannel, IAcknowledgement> AcknowledgementFactory { get; private set; }

    public UnprocessedMessageMode OnUnprocessedMessage { get; private set; } = UnprocessedMessageMode.Requeue;

    public UnprocessedMessageMode OnPoisonMessage { get; private set; } = UnprocessedMessageMode.Requeue;

    IConsumerBuilder IConsumerBuilder.BindToQueue(string queue, Action<IQueueConsumerBuilder> builder)
    {
        var queueConsumer = new QueueConsumerBuilder(queue);
        builder?.Invoke(queueConsumer);
        this.queues.Add(queueConsumer);
        return this;
    }

    IConsumerBuilder IConsumerBuilder.PrefetchSize(uint size)
    {
        this.PrefetchSize = size;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.PrefetchCount(ushort messages)
    {
        this.PrefetchCount = messages;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.ConcurrencyLevel(byte concurrency)
    {
        if (concurrency == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(concurrency));
        }

        this.ConcurrencyLevel = concurrency;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgerFactory)
    {
        if (acknowledgerFactory == null)
        {
            throw new ArgumentNullException(nameof(acknowledgerFactory));
        }

        this.AcknowledgementFactory = acknowledgerFactory;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.OnUnprocessedMessage(UnprocessedMessageMode mode)
    {
        this.OnUnprocessedMessage = mode;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.OnPoisonMessage(UnprocessedMessageMode mode)
    {
        this.OnPoisonMessage = mode;
        return this;
    }

    IConsumerBuilder IConsumerBuilder.MessageHandler(IDeliveredMessageHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        this.handlers.Add(handler);
        return this;
    }
}