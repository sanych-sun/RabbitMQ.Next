using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal sealed class DeliverFrameHandler : IFrameHandler
    {
        private readonly IReadOnlyList<IDeliveredMessageHandler> messageHandlers;
        private readonly IAcknowledger acknowledger;
        private readonly IMethodParser<DeliverMethod> deliverMethodParser;
        private readonly ContentAccessor contentAccessor;
        private readonly UnprocessedMessageMode onUnprocessedMessage;
        private readonly UnprocessedMessageMode onPoisonMessage;

        private bool expectContent;
        private DeliverMethod currentMethod;

        public DeliverFrameHandler(
            ISerializerFactory serializerFactory,
            IAcknowledger acknowledger,
            IMethodParser<DeliverMethod> deliverMethodParser,
            IReadOnlyList<IDeliveredMessageHandler> messageHandlers,
            UnprocessedMessageMode onUnprocessedMessage,
            UnprocessedMessageMode onPoisonMessage)
        {
            this.acknowledger = acknowledger;
            this.messageHandlers = messageHandlers;
            this.onUnprocessedMessage = onUnprocessedMessage;
            this.onPoisonMessage = onPoisonMessage;
            this.deliverMethodParser = deliverMethodParser;
            this.contentAccessor = new ContentAccessor(serializerFactory);
        }

        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId != MethodId.BasicDeliver)
            {
                return new(false);
            }

            this.currentMethod = this.deliverMethodParser.Parse(payload);
            this.expectContent = true;
            return new(true);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (!this.expectContent)
            {
                return new(false);
            }

            return this.HandleDeliveredMessageAsync(properties, contentBytes);
        }

        public void Reset()
        {
            this.expectContent = false;
            this.currentMethod = default;
            this.contentAccessor.Reset();
        }

        private async ValueTask<bool> HandleDeliveredMessageAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            var message = new DeliveredMessage(this.currentMethod.Exchange, this.currentMethod.RoutingKey, this.currentMethod.Redelivered, this.currentMethod.ConsumerTag, this.currentMethod.DeliveryTag);
            this.contentAccessor.Set(contentBytes, properties.ContentType);

            try
            {
                for (var i = 0; i < this.messageHandlers.Count; i++)
                {
                    var handled = await this.messageHandlers[i].TryHandleAsync(message, properties, this.contentAccessor);
                    if (handled)
                    {
                        await this.acknowledger.AckAsync(message.DeliveryTag);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                await this.acknowledger.NackAsync(message.DeliveryTag,this.onPoisonMessage == UnprocessedMessageMode.Requeue);
                return true;
            }
            finally
            {
                this.Reset();
            }

            await this.acknowledger.NackAsync(message.DeliveryTag, this.onUnprocessedMessage == UnprocessedMessageMode.Requeue);
            return true;
        }
    }
}