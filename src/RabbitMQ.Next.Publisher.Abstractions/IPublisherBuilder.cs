using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisherBuilder
    {
        IPublisherBuilder AllowBuffer(int messages);

        IPublisherBuilder UseFormatter(IFormatter formatter);

        IPublisherBuilder UserFormatterSource(IFormatterSource formatters);

        IPublisherBuilder UseTransformer(IMessageTransformer transformer);

        IPublisherBuilder AddReturnedMessageHandler(Func<IReturnedMessage, IContent, bool> returnedMessageHandler);
    }
}