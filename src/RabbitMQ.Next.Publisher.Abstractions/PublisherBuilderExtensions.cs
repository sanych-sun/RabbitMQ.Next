using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IMessageProperties, Content, bool> handler)
        {
            var messageHandler = new ReturnedMessageDelegateHandler(handler);
            builder.AddReturnedMessageHandler(messageHandler);

            return builder;
        }
    }
}