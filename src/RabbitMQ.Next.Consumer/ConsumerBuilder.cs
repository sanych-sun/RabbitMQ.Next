using System;
using System.Collections.Generic;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Consumer;

internal class ConsumerBuilder : IConsumerBuilder
{
    private readonly List<QueueConsumerBuilder> queues = new();
    private readonly SerializerFactory serializerFactory = new();

    public ConsumerBuilder()
    {
        this.AcknowledgementFactory = ch => new DefaultAcknowledgement(ch);
        this.PrefetchCount = 10;
        this.ConcurrencyLevel = 1;
    }
        
    public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

    public ISerializerFactory SerializerFactory => this.serializerFactory;

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

    IConsumerBuilder ISerializationBuilder<IConsumerBuilder>.DefaultSerializer(ISerializer serializer)
    {
        this.serializerFactory.DefaultSerializer(serializer);
        return this;
    }

    IConsumerBuilder ISerializationBuilder<IConsumerBuilder>.UseSerializer(ISerializer serializer, Func<IMessageProperties, bool> predicate)
    {
        this.serializerFactory.UseSerializer(serializer, predicate);
        return this;
    }
}