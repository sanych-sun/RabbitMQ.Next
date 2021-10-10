using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisherBuilder : ISerializationBuilder<IPublisherBuilder>
    {
        IPublisherBuilder UseMessageInitializer(IMessageInitializer initializer);

        IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);

        IPublisherBuilder PublisherConfirms();
    }
}