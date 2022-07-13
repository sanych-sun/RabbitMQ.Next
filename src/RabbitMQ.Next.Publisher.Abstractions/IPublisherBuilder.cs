namespace RabbitMQ.Next.Publisher;

public interface IPublisherBuilder
{
    IPublisherBuilder UseMessageInitializer(IMessageInitializer initializer);

    IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);

    IPublisherBuilder NoConfirm();
}