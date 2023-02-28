using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

public interface IPublisherBuilder: ISerializationBuilder<IPublisherBuilder>
{
    IPublisherBuilder UsePublishMiddleware(Func<IPublishMiddleware, IPublishMiddleware> middlewareFactory);

    IPublisherBuilder OnReturnedMessage(Func<IReturnedMessage,Task> returnedMessageHandler);

    IPublisherBuilder NoConfirms();
}