using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal class Consumer : IConsumer
{
    private readonly IConnection connection;
    private readonly Func<IChannel, IAcknowledgement> acknowledgementFactory;
    private readonly IReadOnlyList<IDeliveredMessageHandler> handlers;
    private readonly IReadOnlyList<QueueConsumerBuilder> queues;
    private readonly uint prefetchSize;
    private readonly ushort prefetchCount;
    private readonly byte concurrencyLevel;
    private readonly UnprocessedMessageMode onUnprocessedMessage;
    private readonly UnprocessedMessageMode onPoisonMessage;

    private IChannel channel;
    private IAcknowledgement acknowledgement;

    public Consumer(
        IConnection connection,
        Func<IChannel, IAcknowledgement> acknowledgementFactory,
        IReadOnlyList<IDeliveredMessageHandler> handlers,
        IReadOnlyList<QueueConsumerBuilder> queues,
        uint prefetchSize,
        ushort prefetchCount,
        byte concurrencyLevel,
        UnprocessedMessageMode onUnprocessedMessage,
        UnprocessedMessageMode onPoisonMessage)
    {
        this.connection = connection;
        this.acknowledgementFactory = acknowledgementFactory;
        this.handlers = handlers;
        this.queues = queues;
        this.prefetchSize = prefetchSize;
        this.prefetchCount = prefetchCount;
        this.concurrencyLevel = concurrencyLevel;
        this.onUnprocessedMessage = onUnprocessedMessage;
        this.onPoisonMessage = onPoisonMessage;
    }

    public async ValueTask DisposeAsync()
        => await this.CancelConsumeAsync();

    public async Task ConsumeAsync(CancellationToken cancellation)
    {
        if (this.channel != null)
        {
            throw new InvalidOperationException("The consumer is already started.");
        }

        await this.InitConsumerAsync();
        await Task.WhenAny(cancellation.AsTask(), this.channel.Completion);
        await this.CancelConsumeAsync();
    }

    private async Task CancelConsumeAsync()
    {
        if (this.channel == null || this.channel.Completion.IsCompleted)
        {
            return;
        }

        for (var i = 0; i < this.queues.Count; i++)
        {
            var queue = this.queues[i];
            await this.channel.SendAsync<CancelMethod, CancelOkMethod>(new CancelMethod(queue.ConsumerTag));
        }

        if (this.acknowledgement != null)
        {
            await this.acknowledgement.DisposeAsync();
        }

        await this.channel.CloseAsync();

        this.acknowledgement = null;
        this.channel = null;
    }

    private async Task InitConsumerAsync()
    {
        this.channel = await this.connection.OpenChannelAsync();
        this.acknowledgement = this.acknowledgementFactory(this.channel);

        var deliverHandler = new DeliverMessageHandler(this.acknowledgement, this.handlers, this.onUnprocessedMessage, this.onPoisonMessage, this.concurrencyLevel);
        this.channel.WithMessageHandler(deliverHandler);

        await this.channel.SendAsync<QosMethod, QosOkMethod>(new QosMethod(this.prefetchSize, this.prefetchCount, false));

        for (var i = 0; i < this.queues.Count; i++)
        {
            var queue = this.queues[i];
            var response = await this.channel.SendAsync<ConsumeMethod, ConsumeOkMethod>(
                new ConsumeMethod(
                    queue.Queue, queue.ConsumerTag, queue.NoLocal, this.acknowledgement == null,
                    queue.Exclusive, queue.Arguments));

            queue.ConsumerTag = response.ConsumerTag;
        }
    }
}