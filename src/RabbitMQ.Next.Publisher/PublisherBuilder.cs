using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<Func<IPublishMiddleware, IPublishMiddleware>> publishMiddlewares = new();
    private readonly List<Func<IReturnMiddleware, IReturnMiddleware>> returnMiddlewares = new();

    public IReadOnlyList<Func<IPublishMiddleware, IPublishMiddleware>> PublishMiddlewares => this.publishMiddlewares;
    
    public IReadOnlyList<Func<IReturnMiddleware, IReturnMiddleware>> ReturnMiddlewares => this.returnMiddlewares;

    public ISerializer Serializer { get; private set; }

    IPublisherBuilder IPublisherBuilder.UsePublishMiddleware(Func<IPublishMiddleware, IPublishMiddleware> middlewareFactory)
    {
        if (middlewareFactory == null)
        {
            throw new ArgumentNullException(nameof(middlewareFactory));
        }

        this.publishMiddlewares.Add(middlewareFactory);
        return this;
    }
    
    IPublisherBuilder IPublisherBuilder.UseReturnMiddleware(Func<IReturnMiddleware, IReturnMiddleware> middlewareFactory)
    {
        if (middlewareFactory == null)
        {
            throw new ArgumentNullException(nameof(middlewareFactory));
        }

        this.returnMiddlewares.Add(middlewareFactory);
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