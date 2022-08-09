using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    private static readonly Task<bool> PositiveTask = Task.FromResult(true);
    private static readonly Task<bool> NegativeTask = Task.FromResult(false);
    
    public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IContent, Task<bool>> handler)
    {
        var messageHandler = new ReturnedMessageDelegateHandler(handler);
        builder.AddReturnedMessageHandler(messageHandler);

        return builder;
    }
    
    public static IPublisherBuilder AddReturnedMessageHandler(this IPublisherBuilder builder, Func<ReturnedMessage, IContent, bool> handler)
    {
        var messageHandler = new ReturnedMessageDelegateHandler(
            (message, content) => handler(message, content) ? PositiveTask: NegativeTask);
        builder.AddReturnedMessageHandler(messageHandler);

        return builder;
    }
}