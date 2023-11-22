using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherBuilder : IPublisherBuilder
{
    private readonly List<Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task>> publishMiddlewares = new();
    private readonly List<Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task>> returnMiddlewares = new();

    public PublisherBuilder(string exchange)
    {
        this.Exchange = exchange;
    }
    
    public bool PublisherConfirms { get; private set; } = true;
    
    public IReadOnlyList<Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task>> PublishMiddlewares
        => this.publishMiddlewares;
    
    public IReadOnlyList<Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task>> ReturnMiddlewares
        => this.returnMiddlewares;

    public string Exchange { get; }

    IPublisherBuilder IPublisherBuilder.UsePublishMiddleware(Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        this.publishMiddlewares.Add(middleware);
        return this;
    }
    
    IPublisherBuilder IPublisherBuilder.UseReturnMiddleware(Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        this.returnMiddlewares.Add(middleware);
        return this;
    }

    public IPublisherBuilder NoConfirms()
    {
        this.PublisherConfirms = false;
        return this;
    }
}