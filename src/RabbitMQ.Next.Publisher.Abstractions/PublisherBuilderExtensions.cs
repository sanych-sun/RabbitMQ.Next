using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IMessageProperties, IContentAccessor, ValueTask<bool>> handler)
        {
            var messageHandler = new ReturnedMessageDelegateHandler(handler);
            builder.AddReturnedMessageHandler(messageHandler);

            return builder;
        }
    }
}