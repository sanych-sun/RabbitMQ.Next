using System;
using RabbitMQ.Next.Consumer.Abstractions.Acknowledgement;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public static class ConsumerBuilderExtensions
    {
        public static IConsumerBuilder EachMessageAcknowledgement(this IConsumerBuilder builder)
        {
            builder.SetAcknowledgement(ack => new EachMessageAcknowledgement(ack));
            return builder;
        }

        public static IConsumerBuilder MultipleMessageAcknowledgement(this IConsumerBuilder builder, TimeSpan timeout)
        {
            builder.SetAcknowledgement(ack => new MultipleMessageAcknowledgement(ack, timeout));
            return builder;
        }
    }
}