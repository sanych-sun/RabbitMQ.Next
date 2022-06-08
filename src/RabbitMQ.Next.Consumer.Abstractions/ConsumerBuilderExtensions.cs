using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer
{
    public static class ConsumerBuilderExtensions
    {
        public static IConsumerBuilder NoAcknowledgement(this IConsumerBuilder builder)
        {
            builder.SetAcknowledgement(_ => null);
            return builder;
        }

        public static IConsumerBuilder MessageHandler(this IConsumerBuilder builder, Func<DeliveredMessage, IContent, ValueTask<bool>> handler)
        {
            var messageHandler = new DeliveredMessageDelegateHandler(handler);
            builder.MessageHandler(messageHandler);

            return builder;
        }

        public static IConsumerBuilder MessageHandler(this IConsumerBuilder builder, Func<DeliveredMessage, IContent, bool> handler)
        {
            var messageHandler = new DeliveredMessageDelegateHandler(
                (message, content) => new ValueTask<bool>(handler(message, content)));
            builder.MessageHandler(messageHandler);

            return builder;
        }
    }
}