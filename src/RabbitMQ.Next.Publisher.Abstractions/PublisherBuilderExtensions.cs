using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IContent, ValueTask<bool>> handler)
    {
        var messageHandler = new ReturnedMessageDelegateHandler(handler);
        builder.AddReturnedMessageHandler(messageHandler);

        return builder;
    }
}