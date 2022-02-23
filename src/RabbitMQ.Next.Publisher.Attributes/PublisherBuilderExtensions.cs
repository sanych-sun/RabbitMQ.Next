namespace RabbitMQ.Next.Publisher.Attributes
{
    public static class PublisherBuilderExtensions
    {
        public static IPublisherBuilder UseAttributesInitializer(this IPublisherBuilder builder)
            => builder.UseMessageInitializer(new AttributeInitializer());
    }
}