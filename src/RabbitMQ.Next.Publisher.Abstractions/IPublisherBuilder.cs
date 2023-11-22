using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public interface IPublisherBuilder
{
    string Exchange { get; }
    
    IPublisherBuilder UsePublishMiddleware(Func<IMessageBuilder,IContentAccessor,Func<IMessageBuilder,IContentAccessor,Task>,Task> middleware);

    IPublisherBuilder UseReturnMiddleware(Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task> middleware);
    
    IPublisherBuilder NoConfirms();
}