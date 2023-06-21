namespace RabbitMQ.Next.Publisher;

public static class PublisherBuilderExtensions
{
    public static IPublisherBuilder PublisherConfirms(this IPublisherBuilder builder)
        => builder.UsePublishMiddleware(next => new ConfirmMessageMiddleware(next));
}