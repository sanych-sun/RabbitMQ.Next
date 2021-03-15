using System;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IContent, bool> handler)
        {
            var messageHandler = new ReturnedMessageDelegateHandler(handler);
            builder.AddReturnedMessageHandler(messageHandler);

            return builder;
        }
    }
}