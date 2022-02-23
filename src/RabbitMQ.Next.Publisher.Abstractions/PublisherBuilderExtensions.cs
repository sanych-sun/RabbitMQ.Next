using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IContentAccessor, ValueTask<bool>> handler)
        {
            var messageHandler = new ReturnedMessageDelegateHandler(handler);
            builder.AddReturnedMessageHandler(messageHandler);

            return builder;
        }
    }
}