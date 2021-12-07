using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class ConsumerBuilder : IConsumerBuilder
    {
        private readonly List<(ISerializer Serializer, string ContentType, bool Default)> serializers = new();
        private readonly List<QueueConsumerBuilder> queues = new();
        private readonly List<IDeliveredMessageHandler> handlers = new();

        public ConsumerBuilder()
        {
            this.AcknowledgementFactory = ch => new DefaultAcknowledgement(ch);
        }

        public IReadOnlyList<(ISerializer Serializer, string ContentType, bool Default)> Serializers => this.serializers;

        public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

        public IReadOnlyList<IDeliveredMessageHandler> Handlers => this.handlers;

        public uint PrefetchSize { get; private set; }

        public ushort PrefetchCount { get; private set; }

        public Func<IChannel, IAcknowledgement> AcknowledgementFactory { get; private set; }

        public UnprocessedMessageMode OnUnprocessedMessage { get; private set; } = UnprocessedMessageMode.Requeue;

        public UnprocessedMessageMode OnPoisonMessage { get; private set; } = UnprocessedMessageMode.Requeue;

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

        IConsumerBuilder IConsumerBuilder.SetAcknowledgement(Func<IChannel, IAcknowledgement> acknowledgerFactory)
        {
            if (acknowledgerFactory == null)
            {
                throw new ArgumentNullException(nameof(acknowledgerFactory));
            }

            this.AcknowledgementFactory = acknowledgerFactory;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.OnUnprocessedMessage(UnprocessedMessageMode mode)
        {
            this.OnUnprocessedMessage = mode;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.OnPoisonMessage(UnprocessedMessageMode mode)
        {
            this.OnPoisonMessage = mode;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.AddMessageHandler(IDeliveredMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.handlers.Add(handler);
            return this;
        }

        public IConsumerBuilder UseSerializer(ISerializer serializer, string contentType = null, bool isDefault = true)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializers.Add((serializer, contentType, isDefault));
            return this;
        }
    }
}