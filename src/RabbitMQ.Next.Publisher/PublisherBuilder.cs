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
        private List<ITypeFormatter> formatters;
        private List<IFormatterSource> formatterSources;
        private List<IReturnedMessageHandler> returnedMessageHandlers;

        public IReadOnlyList<IMessageTransformer> Transformers => this.transformers;

        public IReadOnlyList<ITypeFormatter> Formatters => this.formatters;

        public IReadOnlyList<IFormatterSource> FormatterSources => this.formatterSources;

        public IReadOnlyList<IReturnedMessageHandler> ReturnedMessageHandlers => this.returnedMessageHandlers;

        public bool PublisherConfirms { get; private set; }

        IPublisherBuilder IPublisherBuilder.UseFormatter(ITypeFormatter typeFormatter)
        {
            if (typeFormatter == null)
            {
                throw new ArgumentNullException(nameof(typeFormatter));
            }

            this.formatters ??= new List<ITypeFormatter>();
            this.formatters.Add(typeFormatter);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.UseFormatterSource(IFormatterSource formatters)
        {
            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            this.formatterSources ??= new List<IFormatterSource>();
            this.formatterSources.Add(formatters);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.UseTransformer(IMessageTransformer transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            this.transformers ??= new List<IMessageTransformer>();
            this.transformers.Add(transformer);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler)
        {
            if (returnedMessageHandler == null)
            {
                throw new ArgumentNullException(nameof(returnedMessageHandler));
            }

            this.returnedMessageHandlers ??= new List<IReturnedMessageHandler>();
            this.returnedMessageHandlers.Add(returnedMessageHandler);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.PublisherConfirms()
        {
            this.PublisherConfirms = true;
            return this;
        }
    }
}