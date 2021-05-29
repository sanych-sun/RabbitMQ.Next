using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherBuilderExtensionsTests
    {
        [Fact]
        public void AddReturnedMessageHandlerTests()
        {
            var builder = Substitute.For<IPublisherBuilder>();

            builder.AddReturnedMessageHandler((message, props, content) => new ValueTask<bool>(false));

            builder.Received().AddReturnedMessageHandler(Arg.Any<IReturnedMessageHandler>());
        }
    }
}