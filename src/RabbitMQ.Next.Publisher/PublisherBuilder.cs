using System;
using System.Collections.Generic;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class PublisherBuilder : IPublisherBuilder
    {
        private readonly SerializerFactory serializerFactory = new();
        private List<IMessageInitializer> initializers;
        private List<IReturnedMessageHandler> returnedMessageHandlers;

        public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;

        public ISerializerFactory SerializerFactory => this.serializerFactory;

        public IReadOnlyList<IReturnedMessageHandler> ReturnedMessageHandlers => this.returnedMessageHandlers;

        public bool PublisherConfirms { get; private set; }

        IPublisherBuilder IPublisherBuilder.UseMessageInitializer(IMessageInitializer initializer)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }

            this.initializers ??= new List<IMessageInitializer>();
            this.initializers.Add(initializer);
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

        public IPublisherBuilder UseSerializer(ISerializer serializer, IReadOnlyList<string> contentTypes = null, bool isDefault = true)
        {
            this.serializerFactory.RegisterSerializer(serializer, contentTypes, isDefault);

            return this;
        }
    }
}