using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Consumer : IConsumer
    {
        private readonly IConnection connection;
        private readonly ISerializer serializer;
        private readonly List<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>> handlers;
        private readonly ConsumerInitializer initializer;
        private readonly Func<IAcknowledgement, IAcknowledger> acknowledgerFactory;
        private readonly UnprocessedMessageMode onUnprocessedMessage;
        private readonly UnprocessedMessageMode onPoisonMessage;
        private readonly IMethodHandler frameHandler;

        private IChannel channel;
        private IAcknowledger acknowledger;
        private bool isCancelled;

        public Consumer(
            IConnection connection,
            ISerializer serializer,
            List<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>> handlers,
            ConsumerInitializer initializer,
            Func<IAcknowledgement, IAcknowledger> acknowledgerFactory,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.serializer = serializer;
            this.handlers = handlers;
            this.initializer = initializer;
            this.acknowledgerFactory = acknowledgerFactory;
            this.onUnprocessedMessage = onUnprocessedMessage;
            this.onPoisonMessage = onPoisonMessage;

            this.frameHandler = new MethodHandler<DeliverMethod>(this.HandleMessageAsync);
        }


        public ValueTask DisposeAsync()
            => this.CancelConsumeAsync();

        public async Task ConsumeAsync(CancellationToken cancellation)
        {
            this.channel = await this.connection.OpenChannelAsync(new [] { this.frameHandler }, cancellation);
            if (this.acknowledgerFactory != null)
            {
                var ack = new Acknowledgement(this.channel);
                this.acknowledger = this.acknowledgerFactory(ack);
            }

            await this.channel.UseChannel((this.initializer, cancellation),
                (ch, state) => state.initializer.InitConsumerAsync(ch, state.cancellation),
                cancellation);

            cancellation.Register(() => this.CancelConsumeAsync());

            await this.channel.Completion;
        }

        private async ValueTask CancelConsumeAsync(Exception ex = null)
        {
            this.isCancelled = true;

            await this.channel.UseChannel(this.initializer, (ch, init) => init.CancelAsync(ch));

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
            if (this.isCancelled)
            {
                return true;
            }

            var message = new DeliveredMessage(method.Exchange, method.RoutingKey, method.Redelivered, method.ConsumerTag, method.DeliveryTag);
            var content = new Content(this.serializer, payload);

            try
            {
                for (var i = 0; i < this.handlers.Count; i++)
                {
                    var handled = await this.handlers[i].Invoke(message, properties, content);

                    if (handled)
                    {
                        await this.AckAsync(message);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ProcessPoisonMessageAsync(message, properties, payload, ex);
                return true;
            }
            finally
            {
                content.Dispose();
            }

            await this.ProcessUnprocessedMessageAsync(message, properties, payload);
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

            var requeue = (mode | UnprocessedMessageMode.Requeue) == UnprocessedMessageMode.Requeue;
            return this.acknowledger.NackAsync(message.DeliveryTag, requeue);
        }

        private async ValueTask ProcessPoisonMessageAsync(DeliveredMessage message, IMessageProperties properties, ReadOnlySequence<byte> payload, Exception ex)
        {
            await this.NackAsync(message, this.onPoisonMessage);
            if ((this.onPoisonMessage | UnprocessedMessageMode.StopConsumer) == UnprocessedMessageMode.StopConsumer)
            {
                await this.CancelConsumeAsync(new PoisonMessageException(message, new DetachedMessageProperties(properties), this.MakeDetachedContent(payload), ex));
            }
        }

        private async ValueTask ProcessUnprocessedMessageAsync(DeliveredMessage message, IMessageProperties properties, ReadOnlySequence<byte> payload)
        {
            await this.NackAsync(message, this.onUnprocessedMessage);
            if ((this.onUnprocessedMessage | UnprocessedMessageMode.StopConsumer) == UnprocessedMessageMode.StopConsumer)
            {
                await this.CancelConsumeAsync(new UnprocessedMessageException(message, new DetachedMessageProperties(properties), this.MakeDetachedContent(payload)));
            }
        }

        private Content MakeDetachedContent(ReadOnlySequence<byte> payload)
        {
            var detachedPayload = new byte[payload.Length];
            payload.CopyTo(detachedPayload);
            return new Content(this.serializer, new ReadOnlySequence<byte>(detachedPayload));
        }
    }
}