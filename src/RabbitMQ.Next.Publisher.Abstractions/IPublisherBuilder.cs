using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisherBuilder
    {
        IPublisherBuilder AllowBuffer(int messages);

        IPublisherBuilder UseFormatter(ITypeFormatter typeFormatter);

        IPublisherBuilder UseFormatterSource(IFormatterSource formatters);

        IPublisherBuilder UseTransformer(IMessageTransformer transformer);

        IPublisherBuilder AddReturnedMessageHandler(IReturnedMessageHandler returnedMessageHandler);
    }
}