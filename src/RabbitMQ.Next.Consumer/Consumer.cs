using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Consumer : IConsumer, IAcknowledger
    {
        private readonly IConnection connection;
        private readonly DeliverFrameHandler deliverFrameHandler;
        private readonly Func<IAcknowledgement, IAcknowledger> acknowledgerFactory;
        private readonly IReadOnlyList<QueueConsumerBuilder> queues;
        private readonly uint prefetchSize;
        private readonly ushort prefetchCount;

        private IChannel channel;
        private IAcknowledger acknowledger;

        public Consumer(
            IConnection connection,
            ISerializerFactory serializerFactory,
            Func<IAcknowledgement, IAcknowledger> acknowledgerFactory,
            IReadOnlyList<IDeliveredMessageHandler> handlers,
            IReadOnlyList<QueueConsumerBuilder> queues,
            uint prefetchSize,
            ushort prefetchCount,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.deliverFrameHandler = new DeliverFrameHandler(serializerFactory, this, connection.MethodRegistry, handlers, onUnprocessedMessage, onPoisonMessage);
            this.acknowledgerFactory = acknowledgerFactory;
            this.queues = queues;
            this.prefetchSize = prefetchSize;
            this.prefetchCount = prefetchCount;
        }


        public ValueTask DisposeAsync()
            => this.CancelConsumeAsync();

        public async Task ConsumeAsync(CancellationToken cancellation)
        {
            if (this.channel != null)
            {
                throw new InvalidOperationException("The consumer is already started.");
            }

            this.deliverFrameHandler.Reset();
            this.channel = await this.connection.OpenChannelAsync(new []
            {
                this.deliverFrameHandler
            }, cancellation);

            await this.InitConsumerAsync(cancellation);

            cancellation.Register(() => this.CancelConsumeAsync());

            await this.channel.Completion;
        }

        private async ValueTask CancelConsumeAsync(Exception ex = null)
        {
            for (var i = 0; i < this.queues.Count; i++)
            {
                var queue = this.queues[i];
                await this.channel.SendAsync<CancelMethod, CancelOkMethod>(new CancelMethod(queue.ConsumerTag));
            }

            if (this.acknowledger != null)
            {
                await this.acknowledger.DisposeAsync();
            }

            await this.channel.CloseAsync(ex);

            this.acknowledger = null;
            this.channel = null;
        }

        private async ValueTask InitConsumerAsync(CancellationToken cancellation)
        {
            if (this.acknowledgerFactory != null)
            {
                var ack = new Acknowledgement(this.channel);
                this.acknowledger = this.acknowledgerFactory(ack);
            }

            await this.channel.SendAsync<QosMethod, QosOkMethod>(new QosMethod(this.prefetchSize, this.prefetchCount, false), cancellation);

            for (var i = 0; i < this.queues.Count; i++)
            {
                var queue = this.queues[i];
                var response = await this.channel.SendAsync<ConsumeMethod, ConsumeOkMethod>(
                    new ConsumeMethod(
                        queue.Queue, queue.ConsumerTag, queue.NoLocal, this.acknowledger == null,
                        queue.Exclusive, queue.Arguments), cancellation);

                queue.ConsumerTag = response.ConsumerTag;
            }
        }

        public ValueTask AckAsync(ulong deliveryTag)
            => this.acknowledger?.AckAsync(deliveryTag) ?? default;

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
            => this.acknowledger?.NackAsync(deliveryTag, requeue) ?? default;
    }
}