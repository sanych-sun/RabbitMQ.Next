﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class ConsumerBuilder : IConsumerBuilder
    {
        private readonly List<QueueConsumerBuilder> queues;
        private readonly List<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>> handlers;
        private List<ITypeFormatter> formatters;
        private List<IFormatterSource> formatterSources;

        public ConsumerBuilder()
        {
            this.queues = new List<QueueConsumerBuilder>();
            this.handlers = new List<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>>();
        }

        public IReadOnlyList<ITypeFormatter> Formatters => this.formatters;

        public IReadOnlyList<IFormatterSource> FormatterSources => this.formatterSources;

        public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

        public List<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>> Handlers => this.handlers;

        public uint PrefetchSize { get; private set; }

        public ushort PrefetchCount { get; private set; }

        public Func<IAcknowledgement, IAcknowledger> AcknowledgerFactory { get; private set; }

        public UnprocessedMessageMode OnUnprocessedMessage { get; private set; } = UnprocessedMessageMode.Default;

        public UnprocessedMessageMode OnPoisonMessage { get; private set; } = UnprocessedMessageMode.Default;

        IConsumerBuilder IConsumerBuilder.UseFormatter(ITypeFormatter formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            this.formatters ??= new List<ITypeFormatter>();
            this.formatters.Add(formatter);
            return this;
        }

        IConsumerBuilder IConsumerBuilder.UseFormatterSource(IFormatterSource formatterSource)
        {
            if (formatterSource == null)
            {
                throw new ArgumentNullException(nameof(formatterSource));
            }

            this.formatterSources ??= new List<IFormatterSource>();
            this.formatterSources.Add(formatterSource);
            return this;
        }

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

        IConsumerBuilder IConsumerBuilder.SetAcknowledger(Func<IAcknowledgement, IAcknowledger> acknowledgerFactory)
        {
            if (acknowledgerFactory == null)
            {
                throw new ArgumentNullException(nameof(acknowledgerFactory));
            }

            this.AcknowledgerFactory = acknowledgerFactory;
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

        IConsumerBuilder IConsumerBuilder.AddMessageHandler(Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.handlers.Add(handler);
            return this;
        }
    }
}