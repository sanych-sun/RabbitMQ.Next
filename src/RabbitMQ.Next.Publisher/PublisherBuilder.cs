using System;
using System.Collections.Generic;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class PublisherBuilder : IPublisherBuilder
    {
        private List<IMessageTransformer> transformers;
        private List<IFormatter> formatters;
        private List<IFormatterSource> formatterSources;
        private List<Action<IReturnedMessage, IContent>> returnedMessageHandlers;

        public int BufferSize { get; private set; }

        public IReadOnlyList<IMessageTransformer> Transformers => this.transformers;

        public IReadOnlyList<IFormatter> Formatters => this.formatters;

        public IReadOnlyList<IFormatterSource> FormatterSources => this.formatterSources;

        public IReadOnlyList<Action<IReturnedMessage, IContent>> ReturnedMessageHandlers => this.returnedMessageHandlers;

        IPublisherBuilder IPublisherBuilder.AllowBuffer(int messages)
        {
            if (messages < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(messages));
            }
            this.BufferSize = messages;
            return this;
        }

        IPublisherBuilder IPublisherBuilder.UseFormatter(IFormatter formatter)
        {
            this.formatters ??= new List<IFormatter>();
            this.formatters.Add(formatter);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.UserFormatterSource(IFormatterSource formatters)
        {
            this.formatterSources ??= new List<IFormatterSource>();
            this.formatterSources.Add(formatters);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.UseTransformer(IMessageTransformer transformer)
        {
            this.transformers ??= new List<IMessageTransformer>();
            this.transformers.Add(transformer);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.AddReturnedMessagesHandler(Action<IReturnedMessage, IContent> returnedMessageHandler)
        {
            this.returnedMessageHandlers ??= new List<Action<IReturnedMessage, IContent>>();
            this.returnedMessageHandlers.Add(returnedMessageHandler);
            return this;
        }
    }
}