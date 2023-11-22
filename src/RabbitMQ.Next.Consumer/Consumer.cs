using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal class Consumer : IConsumer
{
    private readonly IConnection connection;
    private readonly Func<IChannel, IAcknowledgement> acknowledgementFactory;
    private readonly IReadOnlyList<QueueConsumerBuilder> queues;
    private readonly IReadOnlyList<Func<IDeliveredMessage, IContentAccessor, Func<IDeliveredMessage, IContentAccessor, Task>, Task>> middlewares;
    private readonly uint prefetchSize;
    private readonly ushort prefetchCount;
    private readonly byte concurrencyLevel;
    private readonly PoisonMessageMode onPoisonMessage;

    private IChannel channel;
    private IAcknowledgement acknowledgement;

    public Consumer(
        IConnection connection,
        Func<IChannel, IAcknowledgement> acknowledgementFactory,
        IReadOnlyList<QueueConsumerBuilder> queues,
        IReadOnlyList<Func<IDeliveredMessage, IContentAccessor, Func<IDeliveredMessage, IContentAccessor, Task>, Task>> middlewares,
        uint prefetchSize,
        ushort prefetchCount,
        byte concurrencyLevel,
        PoisonMessageMode onPoisonMessage)
    {
        this.connection = connection;
        this.acknowledgementFactory = acknowledgementFactory;
        this.queues = queues;
        this.middlewares = middlewares;
        this.prefetchSize = prefetchSize;
        this.prefetchCount = prefetchCount;
        this.concurrencyLevel = concurrencyLevel;
        this.onPoisonMessage = onPoisonMessage;
    }

    public async ValueTask DisposeAsync()
        => await this.CancelConsumeAsync().ConfigureAwait(false);

    public async Task ConsumeAsync(Func<IDeliveredMessage, IContentAccessor, Task> handler, CancellationToken cancellation)
    {
        if (this.channel != null)
        {
            throw new InvalidOperationException("The consumer is already started.");
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        
        var pipeline = handler;
        if (this.middlewares?.Count > 0)
        {
            for (var i = this.middlewares.Count - 1; i >= 0; i--)
            {
                var next = pipeline;
                var middleware = this.middlewares[i];
                pipeline = (m, c) => middleware.Invoke(m, c, next);
            }
        }

        await this.InitConsumerAsync(handler).ConfigureAwait(false);
        
        try
        {
            await Task.WhenAny(cancellation.AsTask(), this.channel.Completion).ConfigureAwait(false);
        }
        finally
        {
            await this.CancelConsumeAsync().ConfigureAwait(false);   
        }
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
            await this.channel.SendAsync<CancelMethod, CancelOkMethod>(new CancelMethod(queue.ConsumerTag)).ConfigureAwait(false);
        }

        if (this.acknowledgement != null)
        {
            await this.acknowledgement.DisposeAsync().ConfigureAwait(false);
        }

        await this.channel.CloseAsync().ConfigureAwait(false);

        this.acknowledgement = null;
        this.channel = null;
    }

    private async Task InitConsumerAsync(Func<IDeliveredMessage, IContentAccessor, Task> handler)
    {
        this.channel = await this.connection.OpenChannelAsync().ConfigureAwait(false);
        this.acknowledgement = this.acknowledgementFactory(this.channel);

        var deliverHandler = new DeliverMessageHandler(handler, this.acknowledgement, this.onPoisonMessage, this.concurrencyLevel);
        this.channel.WithMessageHandler(deliverHandler);

        await this.channel.SendAsync<QosMethod, QosOkMethod>(new QosMethod(this.prefetchSize, this.prefetchCount, false)).ConfigureAwait(false);

        for (var i = 0; i < this.queues.Count; i++)
        {
            var queue = this.queues[i];
            var response = await this.channel.SendAsync<ConsumeMethod, ConsumeOkMethod>(
                new ConsumeMethod(
                    queue.Queue, queue.ConsumerTag, queue.NoLocal, this.acknowledgement == null,
                    queue.Exclusive, queue.Arguments))
                .ConfigureAwait(false);

            queue.ConsumerTag = response.ConsumerTag;
        }
    }
}