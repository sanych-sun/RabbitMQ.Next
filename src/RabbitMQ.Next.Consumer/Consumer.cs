using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Consumer : IConsumer
    {
        private readonly IConnection connection;
        private readonly ISerializerFactory serializerFactory;
        private readonly Func<IChannel, IAcknowledgement> acknowledgementFactory;
        private readonly IReadOnlyList<IDeliveredMessageHandler> handlers;
        private readonly IReadOnlyList<QueueConsumerBuilder> queues;
        private readonly uint prefetchSize;
        private readonly ushort prefetchCount;
        private readonly UnprocessedMessageMode onUnprocessedMessage;
        private readonly UnprocessedMessageMode onPoisonMessage;

        private IChannel channel;
        private IAcknowledgement acknowledgement;

        public Consumer(
            IConnection connection,
            ISerializerFactory serializerFactory,
            Func<IChannel, IAcknowledgement> acknowledgementFactory,
            IReadOnlyList<IDeliveredMessageHandler> handlers,
            IReadOnlyList<QueueConsumerBuilder> queues,
            uint prefetchSize,
            ushort prefetchCount,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.serializerFactory = serializerFactory;
            this.acknowledgementFactory = acknowledgementFactory;
            this.handlers = handlers;
            this.queues = queues;
            this.prefetchSize = prefetchSize;
            this.prefetchCount = prefetchCount;
            this.onUnprocessedMessage = onUnprocessedMessage;
            this.onPoisonMessage = onPoisonMessage;
        }


        public ValueTask DisposeAsync()
            => this.CancelConsumeAsync();

        public async Task ConsumeAsync(CancellationToken cancellation)
        {
            if (this.channel != null)
            {
                throw new InvalidOperationException("The consumer is already started.");
            }

            await this.InitConsumerAsync(cancellation);

            cancellation.Register(() => this.CancelConsumeAsync());

            await this.channel.Completion;
        }

        private async ValueTask CancelConsumeAsync(Exception ex = null)
        {
            if (this.channel == null)
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

            await this.channel.CloseAsync(ex);

            this.acknowledgement = null;
            this.channel = null;
        }

        private async ValueTask InitConsumerAsync(CancellationToken cancellation)
        {
            this.channel = await this.connection.OpenChannelAsync(null, cancellation);

            if (this.acknowledgementFactory != null)
            {
                this.acknowledgement = this.acknowledgementFactory(this.channel);
            }

            var deliverMethodParser = this.connection.MethodRegistry.GetParser<DeliverMethod>();
            var deliverHandler = new DeliverFrameHandler(this.serializerFactory, this.acknowledgement, deliverMethodParser, this.handlers, this.onUnprocessedMessage, this.onPoisonMessage);
            this.channel.AddFrameHandler(deliverHandler);

            await this.channel.SendAsync<QosMethod, QosOkMethod>(new QosMethod(this.prefetchSize, this.prefetchCount, false), cancellation);

            for (var i = 0; i < this.queues.Count; i++)
            {
                var queue = this.queues[i];
                var response = await this.channel.SendAsync<ConsumeMethod, ConsumeOkMethod>(
                    new ConsumeMethod(
                        queue.Queue, queue.ConsumerTag, queue.NoLocal, this.acknowledgement == null,
                        queue.Exclusive, queue.Arguments), cancellation);

                queue.ConsumerTag = response.ConsumerTag;
            }
        }

        public ValueTask AckAsync(ulong deliveryTag)
            => this.acknowledgement?.AckAsync(deliveryTag) ?? default;

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
            => this.acknowledgement?.NackAsync(deliveryTag, requeue) ?? default;
    }
}