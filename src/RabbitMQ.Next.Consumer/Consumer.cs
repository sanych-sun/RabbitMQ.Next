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
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class Consumer : IConsumer
    {
        private readonly IConnection connection;
        private readonly List<Func<DeliveredMessage, IContent, ValueTask<bool>>> handlers;
        private readonly ConsumerInitializer initializer;
        private readonly Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory;
        private readonly UnprocessedMessageMode onUnprocessedMessage;
        private readonly UnprocessedMessageMode onPoisonMessage;
        private readonly IFrameHandler frameHandler;
        private readonly ContentAccessor contentAccessor;
        private readonly TaskCompletionSource<bool> consumeTcs;

        private IChannel channel;
        private IAcknowledgement acknowledgement;

        public Consumer(
            IConnection connection,
            ISerializer serializer,
            List<Func<DeliveredMessage, IContent, ValueTask<bool>>> handlers,
            ConsumerInitializer initializer,
            Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.handlers = handlers;
            this.initializer = initializer;
            this.acknowledgerFactory = acknowledgerFactory;
            this.onUnprocessedMessage = onUnprocessedMessage;
            this.onPoisonMessage = onPoisonMessage;

            var deliverMethodParser = this.connection.MethodRegistry.GetParser<DeliverMethod>();
            this.frameHandler = new ContentFrameHandler<DeliverMethod>((uint)MethodId.BasicDeliver, deliverMethodParser, this.HandleDeliveredMessage, connection.BufferPool);
            this.contentAccessor = new ContentAccessor(serializer);
            this.consumeTcs = new TaskCompletionSource<bool>();
        }


        public ValueTask DisposeAsync()
            => this.CancelConsumeAsync();

        public async Task ConsumeAsync(CancellationToken cancellation)
        {
            // TODO: deal with connection state here
            this.channel = await this.connection.CreateChannelAsync(new IFrameHandler[] { this.frameHandler });
            await this.InitializeConsumerAsync(this.channel);

            cancellation.Register(() => this.CancelConsumeAsync());

            await this.consumeTcs.Task;
        }

        private async Task InitializeConsumerAsync(IChannel channel)
        {
             await channel.UseSyncChannel(this.initializer, (ch, state)
                => state.InitConsumerAsync(ch, CancellationToken.None));

             if (this.acknowledgerFactory != null)
             {
                 var ack = new Acknowledgement(channel);
                 this.acknowledgement = this.acknowledgerFactory(ack);
             }
        }

        private async ValueTask CancelConsumeAsync(Exception ex = null)
        {
            await this.channel.UseSyncChannel(this.initializer, (ch, initializer) =>
                initializer.CancelAsync(ch));

            await this.acknowledgement.DisposeAsync();
            await this.channel.CloseAsync();

            this.acknowledgement = null;
            this.channel = null;

            if (ex == null)
            {
                this.consumeTcs.SetResult(true);
            }
            else
            {
                this.consumeTcs.SetException(ex);
            }
        }


        private void HandleDeliveredMessage(DeliverMethod method, IMessageProperties properties, ReadOnlySequence<byte> content)
        {
            this.HandleMessageAsync(method, properties, content).GetAwaiter().GetResult();
        }

        private async ValueTask HandleMessageAsync(DeliverMethod method, IMessageProperties properties, ReadOnlySequence<byte> content)
        {
            this.contentAccessor.SetPayload(content);
            var message = new DeliveredMessage(method.Exchange, method.RoutingKey, properties, method.Redelivered, method.ConsumerTag, method.DeliveryTag);

            try
            {
                for (var i = 0; i < this.handlers.Count; i++)
                {
                    var handled = await this.handlers[i].Invoke(message, this.contentAccessor);

                    if (handled)
                    {
                        await this.AckAsync(message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ProcessPoisonMessageAsync(message, ex);
                return;
            }

            await this.ProcessUnprocessedMessageAsync(message);
        }

        private async ValueTask AckAsync(DeliveredMessage message)
        {
            if (this.acknowledgement != null)
            {
                await this.acknowledgement.AckAsync(message.DeliveryTag);
            }
        }

        private async ValueTask NackAsync(DeliveredMessage message, UnprocessedMessageMode mode)
        {
            if (this.acknowledgement != null)
            {
                var requeue = (mode | UnprocessedMessageMode.Requeue) == UnprocessedMessageMode.Requeue;
                await this.acknowledgement.NackAsync(message.DeliveryTag, requeue);
            }
        }

        private async ValueTask ProcessPoisonMessageAsync(DeliveredMessage message, Exception ex)
        {
            await this.NackAsync(message, this.onPoisonMessage);
            if ((this.onPoisonMessage | UnprocessedMessageMode.StopConsumer) == UnprocessedMessageMode.StopConsumer)
            {
                await this.CancelConsumeAsync(new PoisonMessageException(message, this.contentAccessor.AsDetached(), ex));
            }
        }

        private async ValueTask ProcessUnprocessedMessageAsync(DeliveredMessage message)
        {
            await this.NackAsync(message, this.onUnprocessedMessage);
            if ((this.onUnprocessedMessage | UnprocessedMessageMode.StopConsumer) == UnprocessedMessageMode.StopConsumer)
            {
                await this.CancelConsumeAsync(new UnprocessedMessageException(message, this.contentAccessor.AsDetached()));
            }
        }
    }
}