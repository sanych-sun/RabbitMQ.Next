using System;
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
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            builder.SetAcknowledger(ack => new MultipleMessageAcknowledger(ack, timeout, count));
            return builder;
        }
    }
}