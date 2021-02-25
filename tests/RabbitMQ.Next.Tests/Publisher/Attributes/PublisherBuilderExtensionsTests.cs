using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class PublisherBuilderExtensionsTests
    {
        [Fact]
        public void UseAttributesTransformer()
        {
            var publisherBuilder = Substitute.For<IPublisherBuilder>();

            publisherBuilder.UseAttributesTransformer();

            publisherBuilder.Received().UseTransformer(Arg.Any<AttributeTransformer>());
        }
    }
}