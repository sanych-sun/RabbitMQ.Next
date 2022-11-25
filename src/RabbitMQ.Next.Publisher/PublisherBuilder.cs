using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<IMessageInitializer> initializers = new();
    private ISerializer serializer;
    private Func<IReturnedMessage,Task> returnedMessageHandler;

    public IReadOnlyList<IMessageInitializer> Initializers => this.initializers;
        
    public Func<IReturnedMessage,Task> ReturnedMessageHandler => this.returnedMessageHandler;

    public ISerializer Serializer => this.serializer;

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

    IPublisherBuilder IPublisherBuilder.OnReturnedMessage(Func<IReturnedMessage,Task> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        this.returnedMessageHandler = handler;
        return this;
    }


    IPublisherBuilder IPublisherBuilder.NoConfirm()
    {
        this.PublisherConfirms = false;
        return this;
    }

    IPublisherBuilder ISerializationBuilder<IPublisherBuilder>.UseSerializer(ISerializer serializer)
    {
        if (serializer == null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }
        
        this.serializer = serializer;
        return this;
    }
}