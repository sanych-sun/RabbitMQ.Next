using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder UseAttributesInitializer(this IPublisherBuilder builder)
            => builder.UseTransformer(new AttributeInitializer());
    }
}