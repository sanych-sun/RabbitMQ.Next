using System;
using System.Collections.Generic;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class PublisherBuilder : IPublisherBuilder
    {
        private List<IMessageInitializer> initializers;
        private Dictionary<string, ISerializer> serializers;
        private List<IReturnedMessageHandler> returnedMessageHandlers;

        public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;

        public IReadOnlyDictionary<string, ISerializer> Serializers => this.serializers;

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

        void ISerializationBuilder.AddSerializer(ISerializer serializer, params string[] contentTypes)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializers ??= new Dictionary<string, ISerializer>();

            if (contentTypes == null || contentTypes.Length == 0)
            {
                this.serializers[string.Empty ] = serializer;
            }
            else
            {
                foreach (var contentType in contentTypes)
                {
                    this.serializers[contentType] = serializer;
                }
            }
        }
    }
}