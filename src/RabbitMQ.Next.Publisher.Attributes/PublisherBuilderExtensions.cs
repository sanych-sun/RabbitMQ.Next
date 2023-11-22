namespace RabbitMQ.Next.Publisher.Attributes;

public static class PublisherBuilderExtensions
{
    public static IPublisherBuilder UseAttributesMiddleware(this IPublisherBuilder builder)
        => builder.UsePublishMiddleware((m, _) => PublisherAttributes.Apply(m));
}