using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer;

public static class ConsumerBuilderExtensions
{
    private static readonly Task<bool> PositiveTask = Task.FromResult(true);
    private static readonly Task<bool> NegativeTask = Task.FromResult(false);
    
    public static IConsumerBuilder NoAcknowledgement(this IConsumerBuilder builder)
    {
        builder.SetAcknowledgement(_ => null);
        return builder;
    }

    public static IConsumerBuilder MessageHandler(this IConsumerBuilder builder, Func<IDeliveredMessage, Task<bool>> handler)
    {
        var messageHandler = new DeliveredMessageDelegateHandler(handler);
        builder.MessageHandler(messageHandler);

        return builder;
    }
    
    public static IConsumerBuilder MessageHandler(this IConsumerBuilder builder, Func<IDeliveredMessage, bool> handler)
    {
        var messageHandler = new DeliveredMessageDelegateHandler(message => handler(message) ? PositiveTask : NegativeTask);
        builder.MessageHandler(messageHandler);

        return builder;
    }
}