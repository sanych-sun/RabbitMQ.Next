using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Publisher;

public static class ConnectionExtensions
{
    private static readonly Func<IPublishMiddleware, IPublishMiddleware> DefaultPublishPipelineFactory = (p) => p;
    private static readonly Func<IReturnMiddleware, IReturnMiddleware> DefaultReturnPipelineFactory = (p) => null;

    public static IPublisher Publisher(this IConnection connection, string exchange, Action<IPublisherBuilder> builder = null)
    {
        if (string.IsNullOrWhiteSpace(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }

        var publisherBuilder = new PublisherBuilder();
        builder?.Invoke(publisherBuilder);

        if (publisherBuilder.Serializer == null)
        {
            throw new InvalidOperationException("Cannot create message publisher without configured serializer. Consider to call UseSerializer.");
        }

        var messagePropsPool = new DefaultObjectPool<MessageBuilder>(
            new MessageBuilderPoolPolicy(),
            10);

        var publishPipelineFactory = DefaultPublishPipelineFactory;
        if (publisherBuilder.PublishMiddlewares.Count > 0)
        {
            publishPipelineFactory = (p) =>
            {
                var last = p;
                for (var i = 0; i < publisherBuilder.PublishMiddlewares.Count; i++)
                {
                    last = publisherBuilder.PublishMiddlewares[i]?.Invoke(last);
                }

                return last;
            };
        }

        var returnPipelineFactory = DefaultReturnPipelineFactory;
        if (publisherBuilder.ReturnMiddlewares.Count > 0)
        {
            returnPipelineFactory = (p) =>
            {
                var last = p;
                for (var i = 0; i < publisherBuilder.ReturnMiddlewares.Count; i++)
                {
                    last = publisherBuilder.ReturnMiddlewares[i]?.Invoke(last);
                }

                return last;
            };
        }

        var publisher = new Publisher(
            connection,
            messagePropsPool,
            publisherBuilder.Serializer,
            exchange,
            publisherBuilder.PublisherConfirms,
            publishPipelineFactory,
            returnPipelineFactory);

        return publisher;
    }
}