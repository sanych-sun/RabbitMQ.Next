using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisherBuilder
    {
        IPublisherBuilder UseFormatter(ITypeFormatter typeFormatter);

        IPublisherBuilder UseTransformer(IMessageInitializer initializer);

        IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);

        IPublisherBuilder PublisherConfirms();
    }
}