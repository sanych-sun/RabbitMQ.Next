using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisherBuilder
    {
        IPublisherBuilder UseFormatter(ITypeFormatter typeFormatter);

        IPublisherBuilder UseTransformer(IMessageTransformer transformer);

        IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);

        IPublisherBuilder PublisherConfirms();
    }
}