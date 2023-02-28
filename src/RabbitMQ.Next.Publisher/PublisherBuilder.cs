using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<Func<IPublishMiddleware, IPublishMiddleware>> publishMiddlewares = new();
    private Func<IReturnedMessage,Task> returnedMessageHandler;

    public IReadOnlyList<Func<IPublishMiddleware, IPublishMiddleware>> PublishMiddlewares => this.publishMiddlewares;
        
    public Func<IReturnedMessage,Task> ReturnedMessageHandler => this.returnedMessageHandler;

    public ISerializer Serializer { get; private set; }

    public bool PublisherConfirms { get; private set; } = true;

    IPublisherBuilder IPublisherBuilder.UsePublishMiddleware(Func<IPublishMiddleware, IPublishMiddleware> middlewareFactory)
    {
        if (middlewareFactory == null)
        {
            throw new ArgumentNullException(nameof(middlewareFactory));
        }

        this.publishMiddlewares.Add(middlewareFactory);
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


    IPublisherBuilder IPublisherBuilder.NoConfirms()
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
        
        this.Serializer = serializer;
        return this;
    }
}