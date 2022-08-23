using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    private static readonly Task<bool> PositiveTask = Task.FromResult(true);
    private static readonly Task<bool> NegativeTask = Task.FromResult(false);
    
    public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<IReturnedMessage, Task<bool>> handler)
    {
        var messageHandler = new ReturnedMessageDelegateHandler(handler);
        builder.AddReturnedMessageHandler(messageHandler);

        return builder;
    }
    
    public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<IReturnedMessage, bool> handler)
    {
        var messageHandler = new ReturnedMessageDelegateHandler(
            message => handler(message) ? PositiveTask: NegativeTask);
        builder.AddReturnedMessageHandler(messageHandler);

        return builder;
    }
}