using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher;

public interface IPublisherBuilder: ISerializationBuilder<IPublisherBuilder>
{
    IPublisherBuilder UseMessageInitializer(IMessageInitializer initializer);

    IPublisherBuilder OnReturnedMessage(Func<IReturnedMessage,Task> returnedMessageHandler);

    IPublisherBuilder NoConfirms();
}