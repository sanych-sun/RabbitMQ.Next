using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public static class ConsumerBuilderExtensions
    {
        public static IConsumerBuilder NoAcknowledgement(this IConsumerBuilder builder)
        {
            builder.SetAcknowledgement(_ => null);
            return builder;
        }

        public static IConsumerBuilder AddMessageHandler(this IConsumerBuilder builder, Func<DeliveredMessage, IContentAccessor, ValueTask<bool>> handler)
        {
            var messageHandler = new DeliveredMessageDelegateHandler(handler);
            builder.AddMessageHandler(messageHandler);

            return builder;
        }
    }
}