using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class ConsumerBuilder : IConsumerBuilder
    {
        private readonly List<QueueConsumerBuilder> queues;
        private readonly List<Func<IDeliveredMessage, IContent, ValueTask<bool>>> handlers;
        private List<IFormatter> formatters;
        private List<IFormatterSource> formatterSources;

        public ConsumerBuilder()
        {
            this.queues = new List<QueueConsumerBuilder>();
            this.handlers = new List<Func<IDeliveredMessage, IContent, ValueTask<bool>>>();
        }

        public IReadOnlyList<IFormatter> Formatters => this.formatters;

        public IReadOnlyList<IFormatterSource> FormatterSources => this.formatterSources;

        public IReadOnlyList<QueueConsumerBuilder> Queues => this.queues;

        public List<Func<IDeliveredMessage, IContent, ValueTask<bool>>> Handlers => this.handlers;

        public uint PrefetchSize { get; private set; }

        public ushort PrefetchCount { get; private set; }

        public Func<IAcknowledgement, IAcknowledgement> AcknowledgerFactory { get; private set; }

        public UnhandledMessageMode OnUnhandledMessage { get; private set; } = UnhandledMessageMode.Default;

        public UnhandledMessageMode OnPoisonMessage { get; private set; } = UnhandledMessageMode.Default;

        IConsumerBuilder IConsumerBuilder.UseFormatter(IFormatter formatter)
        {
            this.formatters ??= new List<IFormatter>();
            this.formatters.Add(formatter);
            return this;
        }

        IConsumerBuilder IConsumerBuilder.UserFormatterSource(IFormatterSource formatters)
        {
            this.formatterSources ??= new List<IFormatterSource>();
            this.formatterSources.Add(formatters);
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

        IConsumerBuilder IConsumerBuilder.SetAcknowledgement(Func<IAcknowledgement, IAcknowledgement> acknowledgerFactory)
        {
            this.AcknowledgerFactory = acknowledgerFactory;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.OnUnhandledMessage(UnhandledMessageMode mode)
        {
            this.OnUnhandledMessage = mode;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.OnPoisonMessage(UnhandledMessageMode mode)
        {
            this.OnPoisonMessage = mode;
            return this;
        }

        IConsumerBuilder IConsumerBuilder.AddMessageHandler(Func<IDeliveredMessage, IContent, ValueTask<bool>> handler)
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