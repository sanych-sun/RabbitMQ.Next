using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Consumer : IConsumer
    {
        private readonly IConnection connection;
        private readonly IReadOnlyList<Func<DeliveredMessage, IMessageProperties, IContentAccessor, ValueTask<bool>>> handlers;
        private readonly Func<IAcknowledgement, IAcknowledger> acknowledgerFactory;
        private readonly IReadOnlyList<QueueConsumerBuilder> queues;
        private readonly uint prefetchSize;
        private readonly ushort prefetchCount;
        private readonly UnprocessedMessageMode onUnprocessedMessage;
        private readonly UnprocessedMessageMode onPoisonMessage;
        private readonly ContentAccessor contentAccessor;

        private IChannel channel;
        private IAcknowledger acknowledger;

        public Consumer(
            IConnection connection,
            ISerializerFactory serializerFactory,
            Func<IAcknowledgement, IAcknowledger> acknowledgerFactory,
            IReadOnlyList<Func<DeliveredMessage, IMessageProperties, IContentAccessor, ValueTask<bool>>> handlers,
            IReadOnlyList<QueueConsumerBuilder> queues,
            uint prefetchSize,
            ushort prefetchCount,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.handlers = handlers;
            this.acknowledgerFactory = acknowledgerFactory;
            this.queues = queues;
            this.prefetchSize = prefetchSize;
            this.prefetchCount = prefetchCount;
            this.onUnprocessedMessage = onUnprocessedMessage;
            this.onPoisonMessage = onPoisonMessage;

            this.contentAccessor = new ContentAccessor(serializerFactory);
        }


        public ValueTask DisposeAsync()
            => this.CancelConsumeAsync();

        public async Task ConsumeAsync(CancellationToken cancellation)
        {
            if (this.channel != null)
            {
                throw new InvalidOperationException("The consumer is already started.");
            }

            this.channel = await this.connection.OpenChannelAsync(new []
            {
                new DeliverFrameHandler(this.connection.MethodRegistry, this.HandleMessageAsync)
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

        private async ValueTask<bool> HandleMessageAsync(DeliverMethod method, IMessageProperties properties, ReadOnlySequence<byte> payload)
        {
            var message = new DeliveredMessage(method.Exchange, method.RoutingKey, method.Redelivered, method.ConsumerTag, method.DeliveryTag);
            this.contentAccessor.Set(payload, properties.ContentType);

            try
            {
                for (var i = 0; i < this.handlers.Count; i++)
                {
                    var handled = await this.handlers[i].Invoke(message, properties, this.contentAccessor);

                    if (handled)
                    {
                        await this.AckAsync(message);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                await this.NackAsync(message, this.onPoisonMessage);
                return true;
            }
            finally
            {
                this.contentAccessor.Reset();
            }

            await this.NackAsync(message, this.onUnprocessedMessage);
            return true;
        }

        private ValueTask AckAsync(DeliveredMessage message)
        {
            if (this.acknowledger == null)
            {
                return default;
            }

            return this.acknowledger.AckAsync(message.DeliveryTag);
        }

        private ValueTask NackAsync(DeliveredMessage message, UnprocessedMessageMode mode)
        {
            if (this.acknowledger == null)
            {
                return default;
            }

            var requeue = mode == UnprocessedMessageMode.Requeue;
            return this.acknowledger.NackAsync(message.DeliveryTag, requeue);
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
    }
}