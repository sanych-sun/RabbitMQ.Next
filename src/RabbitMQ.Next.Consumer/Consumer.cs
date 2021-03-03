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
        private readonly List<Func<DeliveredMessage, IContent, ValueTask<bool>>> handlers;
        private readonly ConsumerInitializer initializer;
        private readonly Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory;
        private readonly UnhandledMessageMode onUnhandledMessage;
        private readonly UnhandledMessageMode onPoisonMessage;
        private readonly DeliveredMessagesFrameHandler frameHandler;
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
            UnhandledMessageMode onUnhandledMessage,
            UnhandledMessageMode onPoisonMessage)
        {
            this.connection = connection;
            this.handlers = handlers;
            this.initializer = initializer;
            this.acknowledgerFactory = acknowledgerFactory;
            this.onUnhandledMessage = onUnhandledMessage;
            this.onPoisonMessage = onPoisonMessage;

            var deliverMethodParser = this.connection.MethodRegistry.GetParser<DeliverMethod>();
            this.frameHandler = new DeliveredMessagesFrameHandler(deliverMethodParser, this.HandleDeliveredMessage);
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

            for (var i = 0; i < this.handlers.Count; i++)
            {
                bool handled ;
                try
                {
                    handled = await this.handlers[i].Invoke(message, this.contentAccessor);
                }
                catch (Exception e)
                {
                    if (this.acknowledgement != null)
                    {
                        await this.acknowledgement.NackAsync(message.DeliveryTag, (this.onPoisonMessage | UnhandledMessageMode.Requeue) == UnhandledMessageMode.Requeue);
                    }

                    // todo: report to diagnostic source
                    if ((this.onUnhandledMessage | UnhandledMessageMode.StopConsumer) == UnhandledMessageMode.StopConsumer)
                    {
                        await this.CancelConsumeAsync(new PoisonMessageException(message, this.contentAccessor.AsDetached(), e));
                    }

                    return;
                }

                if (handled)
                {
                    if (this.acknowledgement != null)
                    {
                        await this.acknowledgement.AckAsync(message.DeliveryTag);
                    }
                    return;
                }
            }

            await this.acknowledgement.NackAsync(message.DeliveryTag, (this.onUnhandledMessage | UnhandledMessageMode.Requeue) == UnhandledMessageMode.Requeue);
            if ((this.onUnhandledMessage | UnhandledMessageMode.StopConsumer) == UnhandledMessageMode.StopConsumer)
            {
                await this.CancelConsumeAsync(new UnhandledMessageException(message, this.contentAccessor.AsDetached()));
            }
        }
    }
}