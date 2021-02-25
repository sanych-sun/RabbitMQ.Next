using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder UseAttributesTransformer(this IPublisherBuilder builder)
            => builder.UseTransformer(new AttributeTransformer());
    }
}