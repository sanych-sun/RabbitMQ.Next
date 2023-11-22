using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

internal class ConsumerBuilder : IConsumerBuilder
{
    private readonly List<QueueConsumerBuilder> queues = new();
    private readonly List<Func<IDeliveredMessage, IContentAccessor, Func<IDeliveredMessage, IContentAccessor, Task>, Task>> middlewares = new();

    public ConsumerBuilder()
    {
        this.AcknowledgementFactory = ch => new DefaultAcknowledgement(ch);
        this.PrefetchCount = 10;
        this.ConcurrencyLevel = 1;
    }
        
    public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

    public IReadOnlyList<Func<IDeliveredMessage, IContentAccessor, Func<IDeliveredMessage, IContentAccessor, Task>, Task>> Middlewares
        => this.middlewares;

    public uint PrefetchSize { get; private set; }

    public ushort PrefetchCount { get; private set; }
        
    public byte ConcurrencyLevel { get; private set; }

    public Func<IChannel, IAcknowledgement> AcknowledgementFactory { get; private set; }

    public PoisonMessageMode OnPoisonMessage { get; private set; } = PoisonMessageMode.Requeue;

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

    IConsumerBuilder IConsumerBuilder.OnPoisonMessage(PoisonMessageMode mode)
    {
        this.OnPoisonMessage = mode;
        return this;
    }
    
    public IConsumerBuilder UseConsumerMiddleware(Func<IDeliveredMessage, IContentAccessor, Func<IDeliveredMessage, IContentAccessor, Task>, Task> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        
        this.middlewares.Add(middleware);
        return this;
    }
}