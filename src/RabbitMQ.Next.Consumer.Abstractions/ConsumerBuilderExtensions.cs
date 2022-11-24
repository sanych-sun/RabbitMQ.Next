namespace RabbitMQ.Next.Consumer;

public static class ConsumerBuilderExtensions
{
    public static IConsumerBuilder NoAcknowledgement(this IConsumerBuilder builder)
    {
        builder.SetAcknowledgement(_ => null);
        return builder;
    }
}