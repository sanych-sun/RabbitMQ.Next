using System;
using System.Collections.Generic;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class PublisherBuilder : IPublisherBuilder
    {
        private List<IMessageInitializer> transformers;
        private List<ITypeFormatter> formatters;
        private List<IReturnedMessageHandler> returnedMessageHandlers;

        public IReadOnlyList<IMessageInitializer> Transformers => this.transformers;

        public IReadOnlyList<ITypeFormatter> Formatters => this.formatters;

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

        IPublisherBuilder IPublisherBuilder.UseMessageInitializer(IMessageInitializer initializer)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }

            this.transformers ??= new List<IMessageInitializer>();
            this.transformers.Add(initializer);
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