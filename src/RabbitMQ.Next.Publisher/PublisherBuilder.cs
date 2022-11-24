using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<IMessageInitializer> initializers = new();
    private readonly SerializerFactory serializerFactory = new();
    private Func<IReturnedMessage,Task> returnedMessageHandler;

    public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;
        
    public Func<IReturnedMessage,Task> ReturnedMessageHandler => this.returnedMessageHandler;

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

    IPublisherBuilder IPublisherBuilder.OnReturnedMessage(Func<IReturnedMessage,Task> returnedMessageHandler)
    {
        if (returnedMessageHandler == null)
        {
            throw new ArgumentNullException(nameof(returnedMessageHandler));
        }

        this.returnedMessageHandler = returnedMessageHandler;
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