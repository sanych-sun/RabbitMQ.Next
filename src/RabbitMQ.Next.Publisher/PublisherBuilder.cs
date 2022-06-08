using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class PublisherBuilder : IPublisherBuilder
    {
        private readonly List<IMessageInitializer> initializers = new();
        private readonly List<IReturnedMessageHandler> returnedMessageHandlers = new();

        public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;
        
        public IReadOnlyList<IReturnedMessageHandler> ReturnedMessageHandlers => this.returnedMessageHandlers;

        public bool PublisherConfirms { get; private set; } = true;

        IPublisherBuilder IPublisherBuilder.UseMessageInitializer(IMessageInitializer initializer)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }

            this.initializers.Add(initializer);
            return this;
        }

        IPublisherBuilder IPublisherBuilder.AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler)
        {
            if (returnedMessageHandler == null)
            {
                throw new ArgumentNullException(nameof(returnedMessageHandler));
            }

            this.returnedMessageHandlers.Add(returnedMessageHandler);
            return this;
        }


        IPublisherBuilder IPublisherBuilder.NoConfirm()
        {
            this.PublisherConfirms = false;
            return this;
        }
    }
}