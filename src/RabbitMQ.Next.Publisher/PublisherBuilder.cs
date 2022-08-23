using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<IMessageInitializer> initializers = new();
    private readonly List<IReturnedMessageHandler> returnedMessageHandlers = new();
    private readonly SerializerFactory serializerFactory = new();

    public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;
        
    public IReadOnlyList<IReturnedMessageHandler> ReturnedMessageHandlers => this.returnedMessageHandlers;

    public ISerializerFactory SerializerFactory => this.serializerFactory;

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

    IPublisherBuilder ISerializationBuilder<IPublisherBuilder>.DefaultSerializer(ISerializer serializer)
    {
        this.serializerFactory.DefaultSerializer(serializer);
        return this;
    }

    IPublisherBuilder ISerializationBuilder<IPublisherBuilder>.UseSerializer(ISerializer serializer, Func<IMessageProperties, bool> predicate)
    {
        this.serializerFactory.UseSerializer(serializer, predicate);
        return this;
    }
}