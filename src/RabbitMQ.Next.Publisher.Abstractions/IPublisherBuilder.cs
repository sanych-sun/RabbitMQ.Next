using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher
{
    public interface IPublisherBuilder : ISerializationBuilder<IPublisherBuilder>
    {
        IPublisherBuilder UseMessageInitializer(IMessageInitializer initializer);

        IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);

        IPublisherBuilder NoConfirm();
    }
}