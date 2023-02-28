namespace RabbitMQ.Next.Publisher.Attributes;

public static class PublisherBuilderExtensions
{
    public static IPublisherBuilder UseAttributesMiddleware(this IPublisherBuilder builder)
        => builder.UsePublishMiddleware(p => new AttributePublishMiddleware(p));
}