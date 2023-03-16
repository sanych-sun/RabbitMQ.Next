using System;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

public interface IPublisherBuilder: ISerializationBuilder<IPublisherBuilder>
{
    IPublisherBuilder UsePublishMiddleware(Func<IPublishMiddleware, IPublishMiddleware> middlewareFactory);

    IPublisherBuilder UseReturnMiddleware(Func<IReturnMiddleware,IReturnMiddleware> middlewareFactory);

    IPublisherBuilder NoConfirms();
}