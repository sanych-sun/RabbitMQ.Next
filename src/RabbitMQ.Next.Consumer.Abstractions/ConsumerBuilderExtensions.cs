using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions.Acknowledger;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public static class ConsumerBuilderExtensions
    {
        public static IConsumerBuilder EachMessageAcknowledgement(this IConsumerBuilder builder)
        {
            builder.SetAcknowledger(ack => new EachMessageAcknowledger(ack));
            return builder;
        }

        public static IConsumerBuilder MultipleMessageAcknowledgement(this IConsumerBuilder builder, TimeSpan timeout, int count)
        {
            builder.SetAcknowledger(ack => new MultipleMessageAcknowledger(ack, timeout, count));
            return builder;
        }

        public static IConsumerBuilder AddMessageHandler(this IConsumerBuilder builder, Func<DeliveredMessage, IMessageProperties, IContentAccessor, ValueTask<bool>> handler)
        {
            var messageHandler = new DeliveredMessageDelegateHandler(handler);
            builder.AddMessageHandler(messageHandler);

            return builder;
        }
    }
}